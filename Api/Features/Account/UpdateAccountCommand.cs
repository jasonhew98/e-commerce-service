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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Api.Features.Account
{
    public class UpdateAccountCommand : IRequest<Result<UpdatedAccountDto, CommandErrorResponse>>
    {
        public string AccountId { get; }
        public string FullName { get; }
        public string Email { get; }
        public List<UpdateAttachment> ProfilePictures { get; }
        public DateTime ModifiedUTCDateTime { get; }

        public UpdateAccountCommand(
            string accountId,
            string fullName,
            string email,
            List<UpdateAttachment> profilePictures,
            DateTime modifiedUTCDateTime)
        {
            AccountId = accountId;
            FullName = fullName;
            Email = email;
            ProfilePictures = profilePictures;
            ModifiedUTCDateTime = modifiedUTCDateTime;
        }
    }

    public class UpdatedAccountDto
    {
        public string AccountId { get; }

        public UpdatedAccountDto(
            string accountId)
        {
            AccountId = accountId;
        }
    }

    public class UpdateAccountCommandValidator : AbstractValidator<UpdateAccountCommand>
    {
        public UpdateAccountCommandValidator()
        {
            RuleFor(a => a.FullName).NotEmpty();
            RuleFor(a => a.Email).NotEmpty();
            RuleFor(a => a.ModifiedUTCDateTime).NotNull().NotEmpty();
        }
    }

    public class UpdateAccountCommandHandler : IRequestHandler<UpdateAccountCommand, Result<UpdatedAccountDto, CommandErrorResponse>>
    {
        private readonly ILogger _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAccountRepository _accountRepository;
        private readonly IAesSecurity _aes;
        private readonly DirectoryPathConfigurationOptions _directoryPathConfiguration;

        public UpdateAccountCommandHandler(
            ILogger<UpdateAccountCommandHandler> logger,
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

        public async Task<Result<UpdatedAccountDto, CommandErrorResponse>> Handle(UpdateAccountCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var account = await _accountRepository.GetAccount(accountId: request.AccountId);

                if (account == null)
                    return ResultYm.NotFound<UpdatedAccountDto>("Product not found.");

                if (!request.ModifiedUTCDateTime.Equals(account.ModifiedUTCDateTime))
                    return ResultYm.Error<UpdatedAccountDto>(CommandErrorResponse.BusinessError(BusinessError.ConcurrencyUpdate.Error()));

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
                    return ResultYm.Error<UpdatedAccountDto>(CommandErrorResponse.BusinessError(BusinessError.FailToUpdateAccount__InvalidFileType.Error()));

                List<Attachment> profilePictures = new List<Attachment>();

                request.ProfilePictures.ForEach(async delegate (UpdateAttachment attachment)
                {
                    var profilePicture = account.ProfilePictures != null ? account.ProfilePictures.FirstOrDefault(p => p.AttachmentId.Equals(attachment.AttachmentId)) : null;

                    if (profilePicture == null)
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
                    }
                    else
                        profilePictures.Add(profilePicture);
                    
                });

                account.UpdateAccountDetails(
                    new Domain.AggregatesModel.AccountAggregate.Account(
                        fullName: request.FullName,
                        email: _aes.Encrypt(request.Email),
                        profilePictures: profilePictures
                    )
                );

                await _accountRepository.UpdateAccount(
                    account: account,
                    user: ("System", "System"),
                    accountId: request.AccountId);

                await _unitOfWork.Commit();

                return ResultYm.Success(new UpdatedAccountDto(account.AccountId));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unknown error has occured while trying to update account details.");
                return ResultYm.Error<UpdatedAccountDto>(ex);
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
