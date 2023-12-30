using Api.Infrastructure;
using Api.Infrastructure.Services;
using Api.Seedwork;
using CSharpFunctionalExtensions;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Requests;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Download;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Api.Internal.Trigger
{
    public class TriggerEventCommand : IRequest<Result<bool, CommandErrorResponse>>
    {
        public JObject GoogleServiceAccountCredential { get; set; }
    }

    public class SampleClass
    {
        public JObject GoogleServiceAccountCredential { get; set; }
    }

    public class FileRequest
    {
        public string FileBase64 { get; set; }
    }

    public class TriggerEventCommandHandler : IRequestHandler<TriggerEventCommand, Result<bool, CommandErrorResponse>>
    {
        private readonly ILogger _logger;
        private readonly DirectoryPathConfigurationOptions _directoryPathConfiguration;
        private readonly BaseAddressConfigurationOptions _baseAddressConfiguration;
        private readonly GoogleAuthService _googleAuthService;
        private readonly GoogleDriveService _googleDriveService;

        public TriggerEventCommandHandler(
            IOptions<DirectoryPathConfigurationOptions> directoryPathConfiguration,
            IOptions<BaseAddressConfigurationOptions> baseAddressConfiguration,
            ILogger<TriggerEventCommandHandler> logger,
            GoogleAuthService googleAuthService,
            GoogleDriveService googleDriveService
        )
        {
            _directoryPathConfiguration = directoryPathConfiguration.Value;
            _baseAddressConfiguration = baseAddressConfiguration.Value;
            _logger = logger;
            _googleAuthService = googleAuthService;
            _googleDriveService = googleDriveService;
        }

        public async Task<Result<bool, CommandErrorResponse>> Handle(TriggerEventCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var url = "https://docs.google.com/spreadsheets/d/1Zdb9LQxNvroq6nEh387guUs0AFdMVpAo/edit?usp=sharing&ouid=114767203686067290889&rtpof=true&sd=true";
                //var fileReq = await DownloadFile(url);
                //await CreateAttachment(fileReq.FileBase64, "Normal.xlsx", _directoryPathConfiguration.ProfilePicture);

                int[] intArray = new int[] { 1, 2, 3, 4, 5};
                int[] copyArray = new int[] { };
                
                for (int i = 0; i < intArray.Length; i++)
                {
                    var test = intArray.Length - (i + 1);
                    var lol = intArray[test];
                    copyArray[i] = intArray[test];
                }


                if (request.GoogleServiceAccountCredential.HasValues)
                {
                    return ResultYm.Success(true);
                }
                var googleAuthRequest = new RefreshTokenRequest
                {
                    GrantType = "refresh_token",
                    ClientId = "",
                    ClientSecret = "",
                    RefreshToken = "",
                    Scope = DriveService.Scope.Drive
                };
                string fileId = "1Zdb9LQxNvroq6nEh387guUs0AFdMVpAo";
                var result = await _googleAuthService.RefreshAccessToken(googleAuthRequest);
                var downloadWithAccessTokenResult = await _googleDriveService.DriveDownloadFileFromAccessToken(result.Value.AccessToken, fileId);
                await CreateAttachment(Convert.ToBase64String(downloadWithAccessTokenResult.Value.ToArray()), "AccessToken.xlsx", _directoryPathConfiguration.ProfilePicture);

                //var downloadWithServiceAccountResult = await _googleDriveService.DriveDownloadFileFromServiceAccount(request.GoogleServiceAccountCredential, fileId);
                //await CreateAttachment(Convert.ToBase64String(downloadWithServiceAccountResult.ToArray()), "ServiceAccount.xlsx", _directoryPathConfiguration.ProfilePicture);

                return ResultYm.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unknown error has occured while trying to trigger event.");
                return ResultYm.Error<bool>(ex);
            }
        }

        public string FormattedDownloadUri(string url)
        {
            var fileId = GetFileIdFromUrl(url);
            if (string.IsNullOrEmpty(fileId))
                return null;

            var gSpreadsheetBaseAddress = _baseAddressConfiguration.GoogleSpreadsheet;
            var downloadUrl = gSpreadsheetBaseAddress + fileId + "/export";

            return downloadUrl.ToString();
        }

        public string GetFileIdFromUrl(string url)
        {
            var gDriveBaseAddress = _baseAddressConfiguration.GoogleDrive;
            var gSpreadsheetBaseAddress = _baseAddressConfiguration.GoogleSpreadsheet;

            if (url.Contains(gDriveBaseAddress))
            {
                url = url.Replace(gDriveBaseAddress, "");
                var fileId = url.Substring(0, url.IndexOf('/'));

                return fileId;
            }

            if (url.Contains(gSpreadsheetBaseAddress))
            {
                url = url.Replace(gSpreadsheetBaseAddress, "");
                var fileId = url.Substring(0, url.IndexOf('/'));

                return fileId;
            }

            //Unlikely to end up here
            return null;
        }

        public async Task<FileRequest> DownloadFile(string url)
        {
            var formattedUrl = FormattedDownloadUri(url);
            using (var client = new HttpClient())
            {
                using (var result = await client.GetAsync(formattedUrl))
                {
                    if (result.IsSuccessStatusCode)
                    {
                        var trueBytes = await result.Content.ReadAsByteArrayAsync();
                        var fileBase64 = Convert.ToBase64String(trueBytes);

                        return new FileRequest
                        {
                            FileBase64 = fileBase64,
                        };
                    }
                }
            }
            return null;
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
