using Api.Infrastructure.Services;
using Api.Model;
using Api.Seedwork;
using CSharpFunctionalExtensions;
using Google.Apis.Auth.OAuth2.Requests;
using Google.Apis.Auth.OAuth2.Responses;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Api.Internal.Microsoft
{
    public class RefreshAccessTokenCommand : IRequest<Result<TokenResponse, CommandErrorResponse>>
    {
        public Credential Credential { get; set; }
    }

    public class RefreshAccessTokenCommandHandler : IRequestHandler<RefreshAccessTokenCommand, Result<TokenResponse, CommandErrorResponse>>
    {
        private readonly ILogger _logger;
        private readonly MicrosoftAuthService _microsoftAuthService;

        public RefreshAccessTokenCommandHandler(
            ILogger<RefreshAccessTokenCommandHandler> logger,
            MicrosoftAuthService microsoftAuthService
        )
        {
            _logger = logger;
            _microsoftAuthService = microsoftAuthService;
        }

        public async Task<Result<TokenResponse, CommandErrorResponse>> Handle(RefreshAccessTokenCommand request, CancellationToken cancellationToken)
        {
            try
            {
                RefreshTokenRequest authRequest = new RefreshTokenRequest
                {
                    ClientId = request.Credential.ClientId,
                    ClientSecret = request.Credential.ClientSecret,
                    RefreshToken = request.Credential.RefreshToken,
                    Scope = "https://graph.microsoft.com/Files.Read.All"
                };

                var result = await _microsoftAuthService.RefreshAccessToken(authRequest);
                if (result.IsFailure)
                    return ResultYm.Error<TokenResponse>(result.Error);

                return ResultYm.Success(result.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unknown error has occured while trying to refresh Microsoft access token.");
                return ResultYm.Error<TokenResponse>(ex);
            }
        }
    }
}
