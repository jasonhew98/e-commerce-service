using Api.Seedwork;
using Api.Seedwork.AesEncryption;
using CSharpFunctionalExtensions;
using Domain.AggregatesModel.AccountAggregate;
using Domain.Model;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Api.Features.Account
{
    public class GetAccountQuery : IRequest<Result<AccountDetailDto, CommandErrorResponse>>
    {
        public string AccountId { get; }

        public GetAccountQuery(
            string accountId)
        {
            AccountId = accountId;
        }
    }

    public class AccountDetailDto
    {
        public string AccountId { get; }
        public string FullName { get; }
        public string Email { get; }
        public List<Attachment> ProfilePictures { get; }
        public DateTime? ModifiedUTCDateTime { get; }

        public AccountDetailDto(
            string accountId,
            string fullName,
            string email,
            DateTime? modifiedUTCDateTime,
            List<Attachment> profilePictures = null)
        {
            AccountId = accountId;
            FullName = fullName;
            Email = email;
            ProfilePictures = profilePictures;
            ModifiedUTCDateTime = modifiedUTCDateTime;
        }
    }

    public class GetAccountQueryHandler : IRequestHandler<GetAccountQuery, Result<AccountDetailDto, CommandErrorResponse>>
    {
        private readonly ILogger _logger;
        private readonly IAccountRepository _accountRepository;
        private readonly IAesSecurity _aes;

        public GetAccountQueryHandler(
            ILogger<GetAccountQueryHandler> logger,
            IAesSecurity aes,
            IAccountRepository accountRepository)
        {
            _logger = logger;
            _aes = aes;
            _accountRepository = accountRepository;
        }

        public async Task<Result<AccountDetailDto, CommandErrorResponse>> Handle(GetAccountQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var account = await _accountRepository.GetAccount(
                    accountId: request.AccountId);

                if (account == null)
                    return ResultYm.NotFound<AccountDetailDto>("Account not found.");

                var result = CreateFromDomain(account);

                return ResultYm.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "An error has occured while trying to get account detail.");
                return ResultYm.Error<AccountDetailDto>(ex);
            }
        }

        public AccountDetailDto CreateFromDomain(Domain.AggregatesModel.AccountAggregate.Account account)
        {
            return new AccountDetailDto(
                accountId: account.AccountId,
                fullName: account.FullName,
                email: _aes.Decrypt(account.Email),
                profilePictures: account.ProfilePictures,
                modifiedUTCDateTime: account.ModifiedUTCDateTime);
        }
    }
}
