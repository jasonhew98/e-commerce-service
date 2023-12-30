using Api.Infrastructure;
using Api.Model;
using Api.Seedwork;
using Api.Seedwork.AesEncryption;
using CSharpFunctionalExtensions;
using Domain;
using Domain.AggregatesModel.UserAggregate;
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

namespace Api.Features.User
{
    public class AddUserCommand : IRequest<Result<bool, CommandErrorResponse>>
    {
        public string UserId { get; }
        public string FullName { get; }
        public string Password { get; }
        public string Email { get; }
        public List<AddAttachment> ProfilePictures { get; }

        public AddUserCommand(
            string fullName,
            string password,
            string email,
            List<AddAttachment> profilePictures = null)
        {
            UserId = Guid.NewGuid().ToString("N");
            FullName = fullName;
            Password = password;
            Email = email;
            ProfilePictures = profilePictures;
        }
    }

    public class AddUserCommandValidator : AbstractValidator<AddUserCommand>
    {
        public AddUserCommandValidator()
        {
            RuleFor(a => a.FullName).NotEmpty();
            RuleFor(a => a.Password).NotEmpty();
            RuleFor(a => a.Email).NotEmpty();
        }
    }

    public class AddUserCommandHandler : IRequestHandler<AddUserCommand, Result<bool, CommandErrorResponse>>
    {
        private readonly ILogger _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserRepository _userRepository;
        private readonly IAesSecurity _aes;
        private readonly DirectoryPathConfigurationOptions _directoryPathConfiguration;

        public AddUserCommandHandler(
            ILogger<AddUserCommandHandler> logger,
            IUnitOfWork unitOfWork,
            IAesSecurity aes,
            IOptions<DirectoryPathConfigurationOptions> directoryPathConfiguration,
            IUserRepository userRepository)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _aes = aes;
            _directoryPathConfiguration = directoryPathConfiguration.Value;
            _userRepository = userRepository;
        }

        public async Task<Result<bool, CommandErrorResponse>> Handle(AddUserCommand request, CancellationToken cancellationToken)
        {
            try
            {
                List<Attachment> profilePictures = new List<Attachment>();

                var user = new Domain.AggregatesModel.UserAggregate.User(
                    userId: request.UserId,
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
                    return ResultYm.Error<bool>(CommandErrorResponse.BusinessError(BusinessError.FailToCreateUser__InvalidFileType.Error()));

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

                _userRepository.Add(user);

                await _unitOfWork.Commit();

                return ResultYm.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unknown error has occured while trying to add new user.");
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
