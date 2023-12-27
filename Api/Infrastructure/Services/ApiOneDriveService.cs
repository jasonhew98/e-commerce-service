using Api.Seedwork;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Api.Infrastructure.Services
{
    public class ApiOneDriveService : OneDriveService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger _logger;

        public ApiOneDriveService(
            HttpClient httpClient,
            ILogger<ApiGoogleAuthService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<Result<byte[], CommandErrorResponse>> DriveDownloadFileFromAccessToken(string accessToken, string encodedUrl)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"shares/{encodedUrl}/driveItem/content");

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var responseMessage = await _httpClient.SendAsync(request);
                var responseContent = await responseMessage.Content.ReadAsByteArrayAsync();

                if (responseMessage.StatusCode != HttpStatusCode.OK)
                    return ResultYm.Error<byte[]>(new Exception(responseMessage.ReasonPhrase + " " + responseContent));

                return ResultYm.Success(responseContent);
            }
            catch (Exception ex)
            {
                return ResultYm.Error<byte[]>(ex);
            }
        }
    }
}
