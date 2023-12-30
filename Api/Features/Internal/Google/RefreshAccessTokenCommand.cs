using Api.Infrastructure.Services;
using Api.Model;
using Api.Seedwork;
using CSharpFunctionalExtensions;
using Google.Apis.Auth.OAuth2.Requests;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Drive.v3;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Api.Internal.Google
{
    public class RefreshAccessTokenCommand : IRequest<Result<TokenResponse, CommandErrorResponse>>
    {
        public Credential Credential { get; set; }
    }

    public class RefreshAccessTokenCommandHandler : IRequestHandler<RefreshAccessTokenCommand, Result<TokenResponse, CommandErrorResponse>>
    {
        private readonly ILogger _logger;
        private readonly GoogleAuthService _googleAuthService;

        public RefreshAccessTokenCommandHandler(
            ILogger<RefreshAccessTokenCommandHandler> logger,
            GoogleAuthService googleAuthService
        )
        {
            _logger = logger;
            _googleAuthService = googleAuthService;
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
                    Scope = DriveService.Scope.Drive
                };

                var result = await _googleAuthService.RefreshAccessToken(authRequest);
                if (result.IsFailure)
                    return ResultYm.Error<TokenResponse>(result.Error);

                return ResultYm.Success(result.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unknown error has occured while trying to refresh Google access token.");
                return ResultYm.Error<TokenResponse>(ex);
            }
        }
    }
}
