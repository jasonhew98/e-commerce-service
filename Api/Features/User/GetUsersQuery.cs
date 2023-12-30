using Api.Seedwork;
using Api.Seedwork.AesEncryption;
using CSharpFunctionalExtensions;
using Domain.AggregatesModel.UserAggregate;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Api.Features.User
{
    public class GetUsersQuery : IRequest<Result<List<UserDto>, CommandErrorResponse>>
    {
        public string SortBy { get; }
        public int SortOrder { get; }
        public int PageSize { get; }
        public int CurrentPage { get; }

        public GetUsersQuery(
            string sortBy,
            int sortOrder,
            int pageSize,
            int currentPage)
        {
            SortBy = sortBy;
            SortOrder = sortOrder;
            PageSize = pageSize;
            CurrentPage = currentPage;
        }
    }

    public class UserDto
    {
        public string UserId { get; }
        public string FullName { get; }
        public string Email { get; }
        public DateTime? ModifiedUTCDateTime { get; }

        public UserDto(
            string userId,
            string fullName,
            string email,
            DateTime? modifiedUTCDateTime)
        {
            UserId = userId;
            FullName = fullName;
            Email = email;
            ModifiedUTCDateTime = modifiedUTCDateTime;
        }
    }

    public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, Result<List<UserDto>, CommandErrorResponse>>
    {
        private readonly ILogger _logger;
        private readonly IUserRepository _userRepository;
        private readonly IAesSecurity _aes;

        public GetUsersQueryHandler(
            ILogger<GetUsersQueryHandler> logger,
            IAesSecurity aes,
            IUserRepository userRepository)
        {
            _logger = logger;
            _aes = aes;
            _userRepository = userRepository;
        }

        public async Task<Result<List<UserDto>, CommandErrorResponse>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var users = await _userRepository.GetUsers(
                    limit: request.PageSize,
                    offset: request.PageSize * (request.CurrentPage < 1 ? 0 : request.CurrentPage - 1),
                    sortBy: request.SortBy,
                    sortOrder: request.SortOrder);

                if (users == null)
                    return ResultYm.Success(new List<UserDto>());

                var result = users.Select(p => CreateFromDomain(p)).ToList();

                return ResultYm.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "An error has occured while trying to get user list.");
                return ResultYm.Error<List<UserDto>>(ex);
            }
        }

        public UserDto CreateFromDomain(Domain.AggregatesModel.UserAggregate.User user)
        {
            return new UserDto(
                userId: user.UserId,
                fullName: user.FullName,
                email: _aes.Decrypt(user.Email),
                modifiedUTCDateTime: user.ModifiedUTCDateTime);
        }
    }
}
