using Api.Model;
using Api.Seedwork;
using CSharpFunctionalExtensions;
using Domain.AggregatesModel.AccountAggregate;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Api.Features.Account
{
    public class GetAccountPageSizeQuery : IRequest<Result<PageSizeDto, CommandErrorResponse>>
    {
        public int PageSize { get; }

        public GetAccountPageSizeQuery(
            int pageSize)
        {
            PageSize = pageSize;
        }
    }

    public class GetAccountPageSizeQueryHandler : IRequestHandler<GetAccountPageSizeQuery, Result<PageSizeDto, CommandErrorResponse>>
    {
        private readonly ILogger _logger;
        private readonly IAccountRepository _accountRepository;

        public GetAccountPageSizeQueryHandler(
            ILogger<GetAccountPageSizeQueryHandler> logger,
            IAccountRepository accountRepository)
        {
            _logger = logger;
            _accountRepository = accountRepository;
        }

        public async Task<Result<PageSizeDto, CommandErrorResponse>> Handle(GetAccountPageSizeQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var count = await _accountRepository.GetAccountCount();

                var result = new PageSizeDto(count, request.PageSize);

                return ResultYm.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "An error has occured while trying to get account page size");
                return ResultYm.Error<PageSizeDto>(ex);
            }
        }
    }
}
