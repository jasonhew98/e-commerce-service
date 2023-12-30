using Api.Infrastructure;
using Api.Infrastructure.Services;
using Api.Model;
using Api.Seedwork;
using CSharpFunctionalExtensions;
using Google.Apis.Auth.OAuth2.Requests;
using Google.Apis.Drive.v3;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Api.Internal.Google
{
    public class DownloadDriveFileCommand : IRequest<Result<bool, CommandErrorResponse>>
    {
        public string FileName { get; set; }
        public string Url { get; set; }
        public Credential Credential { get; set; }
    }

    public class DownloadDriveFileCommandHandler : IRequestHandler<DownloadDriveFileCommand, Result<bool, CommandErrorResponse>>
    {
        private readonly ILogger _logger;
        private readonly DirectoryPathConfigurationOptions _directoryPathConfiguration;
        private readonly GoogleAuthService _googleAuthService;
        private readonly GoogleDriveService _googleDriveService;
        private readonly FileService _fileService;

        public DownloadDriveFileCommandHandler(
            IOptions<DirectoryPathConfigurationOptions> directoryPathConfiguration,
            ILogger<DownloadDriveFileCommandHandler> logger,
            GoogleAuthService googleAuthService,
            GoogleDriveService googleDriveService,
            FileService fileService
        )
        {
            _directoryPathConfiguration = directoryPathConfiguration.Value;
            _logger = logger;
            _googleAuthService = googleAuthService;
            _googleDriveService = googleDriveService;
            _fileService = fileService;
        }

        public async Task<Result<bool, CommandErrorResponse>> Handle(DownloadDriveFileCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var fileDownloadResult = await DownloadFile(request);
                if (fileDownloadResult.IsFailure)
                    return ResultYm.Error<bool>(fileDownloadResult.Error);

                await _fileService.CreateFile(Convert.ToBase64String(fileDownloadResult.Value.ToArray()), request.FileName, _directoryPathConfiguration.Download);

                return ResultYm.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unknown error has occured while trying to trigger event.");
                return ResultYm.Error<bool>(ex);
            }
        }

        public async Task<Result<MemoryStream, CommandErrorResponse>> DownloadFile(DownloadDriveFileCommand request)
        {
            var fileId = _fileService.GetFileIdFromUrl(request.Url);

            if (request.Credential.HasServiceAccount())
            {
                var downloadWithServiceAccountResult = await _googleDriveService.DriveDownloadFileFromServiceAccount(request.Credential.ServiceAccount, fileId.Value);
                if (downloadWithServiceAccountResult.IsFailure)
                    return ResultYm.Error<MemoryStream>(downloadWithServiceAccountResult.Error);
                return downloadWithServiceAccountResult.Value;
            }

            var downloadWithAccessTokenResult = await _googleDriveService.DriveDownloadFileFromAccessToken(request.Credential.AccessToken, fileId.Value);
            if (downloadWithAccessTokenResult.IsFailure)
            {
                RefreshTokenRequest authRequest = new RefreshTokenRequest
                {
                    GrantType = "refresh_token",
                    ClientId = request.Credential.ClientId,
                    ClientSecret = request.Credential.ClientSecret,
                    RefreshToken = request.Credential.RefreshToken,
                    Scope = DriveService.Scope.Drive
                };

                var result = await _googleAuthService.RefreshAccessToken(authRequest);
                if (result.IsFailure)
                    return ResultYm.Error<MemoryStream>(result.Error);

                var retryDownloadWithAccessTokenResult = await _googleDriveService.DriveDownloadFileFromAccessToken(result.Value.AccessToken, fileId.Value);
                if (retryDownloadWithAccessTokenResult.IsFailure)
                    return ResultYm.Error<MemoryStream>(retryDownloadWithAccessTokenResult.Error);

                return retryDownloadWithAccessTokenResult.Value;
            }

            return downloadWithAccessTokenResult.Value;
        }
    }
}
