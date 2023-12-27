using Api.Seedwork;
using CSharpFunctionalExtensions;
using Google.Apis.Auth.OAuth2.Requests;
using Google.Apis.Auth.OAuth2.Responses;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Api.Infrastructure.Services
{
    public class ApiMicrosoftAuthService : MicrosoftAuthService
    {

        private readonly HttpClient _httpClient;
        private readonly ILogger _logger;

        public ApiMicrosoftAuthService(
            HttpClient httpClient,
            ILogger<ApiMicrosoftAuthService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<Result<TokenResponse, CommandErrorResponse>> RefreshAccessToken(RefreshTokenRequest authRequest)
        {
            try
            {
                // Apparently Microsoft's OAuth API only accepts FormUrlEncoded body
                var parameters = new Dictionary<string, string>();
                parameters.Add("client_id", authRequest.ClientId);
                parameters.Add("client_secret", authRequest.ClientSecret);
                parameters.Add("grant_type", authRequest.GrantType);
                parameters.Add("scope", authRequest.Scope);
                parameters.Add("refresh_token", authRequest.RefreshToken);

                var responseMessage = await _httpClient.PostAsync($"token", new FormUrlEncodedContent(parameters));
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
