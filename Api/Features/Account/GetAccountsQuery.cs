using Api.Seedwork;
using Api.Seedwork.AesEncryption;
using CSharpFunctionalExtensions;
using Domain.AggregatesModel.AccountAggregate;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Api.Features.Account
{
    public class GetAccountsQuery : IRequest<Result<List<AccountDto>, CommandErrorResponse>>
    {
        public string SortBy { get; }
        public int SortOrder { get; }
        public int PageSize { get; }
        public int CurrentPage { get; }

        public GetAccountsQuery(
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

    public class AccountDto
    {
        public string AccountId { get; }
        public string FullName { get; }
        public string Email { get; }
        public DateTime? ModifiedUTCDateTime { get; }

        public AccountDto(
            string accountId,
            string fullName,
            string email,
            DateTime? modifiedUTCDateTime)
        {
            AccountId = accountId;
            FullName = fullName;
            Email = email;
            ModifiedUTCDateTime = modifiedUTCDateTime;
        }
    }

    public class GetAccountsQueryHandler : IRequestHandler<GetAccountsQuery, Result<List<AccountDto>, CommandErrorResponse>>
    {
        private readonly ILogger _logger;
        private readonly IAccountRepository _accountRepository;
        private readonly IAesSecurity _aes;

        public GetAccountsQueryHandler(
            ILogger<GetAccountsQueryHandler> logger,
            IAesSecurity aes,
            IAccountRepository accountRepository)
        {
            _logger = logger;
            _aes = aes;
            _accountRepository = accountRepository;
        }

        public async Task<Result<List<AccountDto>, CommandErrorResponse>> Handle(GetAccountsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var accounts = await _accountRepository.GetAccounts(
                    limit: request.PageSize,
                    offset: request.PageSize * (request.CurrentPage < 1 ? 0 : request.CurrentPage - 1),
                    sortBy: request.SortBy,
                    sortOrder: request.SortOrder);

                if (accounts == null)
                    return ResultYm.Success(new List<AccountDto>());

                var result = accounts.Select(p => CreateFromDomain(p)).ToList();

                return ResultYm.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "An error has occured while trying to get account list.");
                return ResultYm.Error<List<AccountDto>>(ex);
            }
        }

        public AccountDto CreateFromDomain(Domain.AggregatesModel.AccountAggregate.Account account)
        {
            return new AccountDto(
                accountId: account.AccountId,
                fullName: account.FullName,
                email: _aes.Decrypt(account.Email),
                modifiedUTCDateTime: account.ModifiedUTCDateTime);
        }
    }
}
