using Api.Model;
using Api.Seedwork;
using CSharpFunctionalExtensions;
using Domain.AggregatesModel.UserAggregate;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Api.Features.User
{
    public class GetUserPageSizeQuery : IRequest<Result<PageSizeDto, CommandErrorResponse>>
    {
        public int PageSize { get; }

        public GetUserPageSizeQuery(
            int pageSize)
        {
            PageSize = pageSize;
        }
    }

    public class GetUserPageSizeQueryHandler : IRequestHandler<GetUserPageSizeQuery, Result<PageSizeDto, CommandErrorResponse>>
    {
        private readonly ILogger _logger;
        private readonly IUserRepository _userRepository;

        public GetUserPageSizeQueryHandler(
            ILogger<GetUserPageSizeQueryHandler> logger,
            IUserRepository userRepository)
        {
            _logger = logger;
            _userRepository = userRepository;
        }

        public async Task<Result<PageSizeDto, CommandErrorResponse>> Handle(GetUserPageSizeQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var count = await _userRepository.GetUserCount();

                var result = new PageSizeDto(count, request.PageSize);

                return ResultYm.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "An error has occured while trying to get user page size");
                return ResultYm.Error<PageSizeDto>(ex);
            }
        }
    }
}
