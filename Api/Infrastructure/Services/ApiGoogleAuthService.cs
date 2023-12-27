using Api.Seedwork;
using CSharpFunctionalExtensions;
using Google.Apis.Auth.OAuth2.Requests;
using Google.Apis.Auth.OAuth2.Responses;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Api.Infrastructure.Services
{
    public class ApiGoogleAuthService : GoogleAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger _logger;

        public ApiGoogleAuthService(
            HttpClient httpClient,
            ILogger<ApiGoogleAuthService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<Result<TokenResponse, CommandErrorResponse>> RefreshAccessToken(RefreshTokenRequest googleAuthRequest)
        {
            try
            {
                var serializedObject = JsonConvert.SerializeObject(googleAuthRequest);

                var httpContent = new StringContent(serializedObject, Encoding.UTF8, "application/json");

                var responseMessage = await _httpClient.PostAsync($"token", httpContent);
                var responseContent = await responseMessage.Content.ReadAsStringAsync();

                if (responseMessage.StatusCode != HttpStatusCode.OK)
                    return ResultYm.Error<TokenResponse>(new Exception(responseMessage.ReasonPhrase + " " + responseContent));

                var result = JsonConvert.DeserializeObject<TokenResponse>(responseContent);

                return result;
            }
            catch (Exception ex)
            {
                return ResultYm.Error<TokenResponse>(ex);
            }
        }
    }
}
