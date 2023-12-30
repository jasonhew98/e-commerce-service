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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Api.Features.User
{
    public class UpdateUserCommand : IRequest<Result<UpdatedUserDto, CommandErrorResponse>>
    {
        public string UserId { get; }
        public string FullName { get; }
        public string Email { get; }
        public List<UpdateAttachment> ProfilePictures { get; }
        public DateTime ModifiedUTCDateTime { get; }

        public UpdateUserCommand(
            string userId,
            string fullName,
            string email,
            List<UpdateAttachment> profilePictures,
            DateTime modifiedUTCDateTime)
        {
            UserId = userId;
            FullName = fullName;
            Email = email;
            ProfilePictures = profilePictures;
            ModifiedUTCDateTime = modifiedUTCDateTime;
        }
    }

    public class UpdatedUserDto
    {
        public string UserId { get; }

        public UpdatedUserDto(
            string userId)
        {
            UserId = userId;
        }
    }

    public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
    {
        public UpdateUserCommandValidator()
        {
            RuleFor(a => a.FullName).NotEmpty();
            RuleFor(a => a.Email).NotEmpty();
            RuleFor(a => a.ModifiedUTCDateTime).NotNull().NotEmpty();
        }
    }

    public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, Result<UpdatedUserDto, CommandErrorResponse>>
    {
        private readonly ILogger _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserRepository _userRepository;
        private readonly IAesSecurity _aes;
        private readonly DirectoryPathConfigurationOptions _directoryPathConfiguration;

        public UpdateUserCommandHandler(
            ILogger<UpdateUserCommandHandler> logger,
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

        public async Task<Result<UpdatedUserDto, CommandErrorResponse>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _userRepository.GetUser(userId: request.UserId);

                if (user == null)
                    return ResultYm.NotFound<UpdatedUserDto>("Product not found.");

                if (!request.ModifiedUTCDateTime.Equals(user.ModifiedUTCDateTime))
                    return ResultYm.Error<UpdatedUserDto>(CommandErrorResponse.BusinessError(BusinessError.ConcurrencyUpdate.Error()));

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
                    return ResultYm.Error<UpdatedUserDto>(CommandErrorResponse.BusinessError(BusinessError.FailToUpdateUser__InvalidFileType.Error()));

                List<Attachment> profilePictures = new List<Attachment>();

                request.ProfilePictures.ForEach(async delegate (UpdateAttachment attachment)
                {
                    var profilePicture = user.ProfilePictures != null ? user.ProfilePictures.FirstOrDefault(p => p.AttachmentId.Equals(attachment.AttachmentId)) : null;

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

                user.UpdateUserDetails(
                    new Domain.AggregatesModel.UserAggregate.User(
                        fullName: request.FullName,
                        email: _aes.Encrypt(request.Email),
                        profilePictures: profilePictures
                    )
                );

                await _userRepository.UpdateUser(
                    user: user,
                    currentUser: ("System", "System"),
                    userId: request.UserId);

                await _unitOfWork.Commit();

                return ResultYm.Success(new UpdatedUserDto(user.UserId));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unknown error has occured while trying to update user details.");
                return ResultYm.Error<UpdatedUserDto>(ex);
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
