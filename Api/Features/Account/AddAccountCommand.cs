using Api.Infrastructure;
using Api.Model;
using Api.Seedwork;
using Api.Seedwork.AesEncryption;
using CSharpFunctionalExtensions;
using Domain;
using Domain.AggregatesModel.AccountAggregate;
using Domain.Model;
using FluentValidation;
using Infrastructure.Seedwork;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Api.Features.Account
{
    public class AddAccountCommand : IRequest<Result<bool, CommandErrorResponse>>
    {
        public string AccountId { get; }
        public string FullName { get; }
        public string Password { get; }
        public string Email { get; }
        public List<AddAttachment> ProfilePictures { get; }

        public AddAccountCommand(
            string fullName,
            string password,
            string email,
            List<AddAttachment> profilePictures = null)
        {
            AccountId = Guid.NewGuid().ToString("N");
            FullName = fullName;
            Password = password;
            Email = email;
            ProfilePictures = profilePictures;
        }
    }

    public class AddAccountCommandValidator : AbstractValidator<AddAccountCommand>
    {
        public AddAccountCommandValidator()
        {
            RuleFor(a => a.FullName).NotEmpty();
            RuleFor(a => a.Password).NotEmpty();
            RuleFor(a => a.Email).NotEmpty();
        }
    }

    public class AddAccountCommandHandler : IRequestHandler<AddAccountCommand, Result<bool, CommandErrorResponse>>
    {
        private readonly ILogger _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAccountRepository _accountRepository;
        private readonly IAesSecurity _aes;
        private readonly DirectoryPathConfigurationOptions _directoryPathConfiguration;

        public AddAccountCommandHandler(
            ILogger<AddAccountCommandHandler> logger,
            IUnitOfWork unitOfWork,
            IAesSecurity aes,
            IOptions<DirectoryPathConfigurationOptions> directoryPathConfiguration,
            IAccountRepository accountRepository)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _aes = aes;
            _directoryPathConfiguration = directoryPathConfiguration.Value;
            _accountRepository = accountRepository;
        }

        public async Task<Result<bool, CommandErrorResponse>> Handle(AddAccountCommand request, CancellationToken cancellationToken)
        {
            try
            {
                List<Attachment> profilePictures = new List<Attachment>();

                var account = new Domain.AggregatesModel.AccountAggregate.Account(
                    accountId: request.AccountId,
                    fullName: request.FullName,
                    password: _aes.Encrypt(request.Password),
                    email: _aes.Encrypt(request.Email),
                    profilePictures: profilePictures,
                    createdBy: "System",
                    createdByName: "System",
                    createdUTCDateTime: DateTime.Now,
                    modifiedBy: "System",
                    modifiedByName: "System",
                    modifiedUTCDateTime: DateTime.Now);

                var valid = true;

                if (request.ProfilePictures.Count > 0)
                {
                    string[] supportedFileTypes = { "image/jpg", "image/png", "image/jpeg" };

                    request.ProfilePictures.ForEach(x =>
                    {
                        if (valid && Array.IndexOf(supportedFileTypes, x.BlobType) == -1)
                        {
                            valid = false;
                        }
                    });
                }

                if (!valid)
                    return ResultYm.Error<bool>(CommandErrorResponse.BusinessError(BusinessError.FailToCreateAccount__InvalidFileType.Error()));

                request.ProfilePictures.ForEach(async delegate (AddAttachment attachment)
                {
                    var newAttachmentId = Guid.NewGuid().ToString("N");
                    var attachmentFileName = $"{newAttachmentId}-{attachment.AttachmentFileName}";

                    profilePictures.Add(
                        new Attachment(
                            attachmentId: newAttachmentId,
                            name: attachmentFileName,
                            blobType: attachment.BlobType
                        )
                    );

                    await CreateAttachment(attachment.AttachmentBase64, attachmentFileName, _directoryPathConfiguration.ProfilePicture);
                });

                _accountRepository.Add(account);

                await _unitOfWork.Commit();

                return ResultYm.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unknown error has occured while trying to add new account.");
                return ResultYm.Error<bool>(ex);
            }
        }

        public async Task CreateAttachment(string base64, string fileName, string folderPath)
        {
            try
            {
                if (!Directory.Exists(folderPath))
                    throw new DirectoryNotFoundException($@"Directory path does not exist: {folderPath}");

                await File.WriteAllBytesAsync(Path.Combine(folderPath, fileName), Convert.FromBase64String(base64));
            }
            catch
            {
                throw;
            }
        }
    }
}
