using Api.Seedwork;
using Api.Seedwork.AesEncryption;
using CSharpFunctionalExtensions;
using Domain.AggregatesModel.UserAggregate;
using Domain.Model;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Api.Features.User
{
    public class GetUserQuery : IRequest<Result<UserDetailDto, CommandErrorResponse>>
    {
        public string UserId { get; }

        public GetUserQuery(
            string userId)
        {
            UserId = userId;
        }
    }

    public class UserDetailDto
    {
        public string UserId { get; }
        public string FullName { get; }
        public string Email { get; }
        public List<Attachment> ProfilePictures { get; }
        public DateTime? ModifiedUTCDateTime { get; }

        public UserDetailDto(
            string userId,
            string fullName,
            string email,
            DateTime? modifiedUTCDateTime,
            List<Attachment> profilePictures = null)
        {
            UserId = userId;
            FullName = fullName;
            Email = email;
            ProfilePictures = profilePictures;
            ModifiedUTCDateTime = modifiedUTCDateTime;
        }
    }

    public class GetUserQueryHandler : IRequestHandler<GetUserQuery, Result<UserDetailDto, CommandErrorResponse>>
    {
        private readonly ILogger _logger;
        private readonly IUserRepository _userRepository;
        private readonly IAesSecurity _aes;

        public GetUserQueryHandler(
            ILogger<GetUserQueryHandler> logger,
            IAesSecurity aes,
            IUserRepository userRepository)
        {
            _logger = logger;
            _aes = aes;
            _userRepository = userRepository;
        }

        public async Task<Result<UserDetailDto, CommandErrorResponse>> Handle(GetUserQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _userRepository.GetUser(
                    userId: request.UserId);

                if (user == null)
                    return ResultYm.NotFound<UserDetailDto>("User not found.");

                var result = CreateFromDomain(user);

                return ResultYm.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "An error has occured while trying to get user detail.");
                return ResultYm.Error<UserDetailDto>(ex);
            }
        }

        public UserDetailDto CreateFromDomain(Domain.AggregatesModel.UserAggregate.User user)
        {
            return new UserDetailDto(
                userId: user.UserId,
                fullName: user.FullName,
                email: _aes.Decrypt(user.Email),
                profilePictures: user.ProfilePictures,
                modifiedUTCDateTime: user.ModifiedUTCDateTime);
        }
    }
}
