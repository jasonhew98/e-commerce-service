using Api.Seedwork;
using CSharpFunctionalExtensions;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Download;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Api.Infrastructure.Services
{
    public class ApiGoogleDriveService : GoogleDriveService
    {

        public ApiGoogleDriveService()
        {

        }

        public async Task<Result<MemoryStream, CommandErrorResponse>> DriveDownloadFileFromServiceAccount(JObject serviceAccCreds, string fileId)
        {
            try
            {
                var serializedCredential = JsonConvert.SerializeObject(serviceAccCreds);
                var credentialInBytes = Encoding.ASCII.GetBytes(serializedCredential);
                var credentialStream = new MemoryStream(credentialInBytes);

                var serviceAcc = ServiceAccountCredential.FromServiceAccountData(credentialStream);
                var googleCredential = GoogleCredential.FromServiceAccountCredential(serviceAcc)
                    .CreateScoped(DriveService.Scope.Drive);

                // Create Drive API service.
                var service = new DriveService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = googleCredential
                });

                var req = service.Files.Get(fileId);
                var stream = new MemoryStream();

                req.MediaDownloader.ProgressChanged +=
                    progress =>
                    {
                        switch (progress.Status)
                        {
                            case DownloadStatus.Failed:
                                {
                                    Console.WriteLine("Download failed.");
                                    break;
                                }
                        }
                    };

                req.Download(stream);
                return stream;
            }
            catch (Exception ex)
            {
                return ResultYm.Error<MemoryStream>(ex);
            }
        }

        public async Task<Result<MemoryStream, CommandErrorResponse>> DriveDownloadFileFromAccessToken(string accessToken, string fileId)
        {
            try
            {
                var googleCredential = GoogleCredential.FromAccessToken(accessToken)
                    .CreateScoped(DriveService.Scope.Drive);

                // Create Drive API service.
                var service = new DriveService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = googleCredential
                });

                var req = service.Files.Get(fileId);
                var stream = new MemoryStream();

                req.MediaDownloader.ProgressChanged +=
                    progress =>
                    {
                        switch (progress.Status)
                        {
                            case DownloadStatus.Failed:
                                {
                                    Console.WriteLine("Download failed.");
                                    break;
                                }
                        }
                    };

                req.Download(stream);
                return stream;
            }
            catch (Exception ex)
            {
                return ResultYm.Error<MemoryStream>(ex);
            }
        }
    }
}
