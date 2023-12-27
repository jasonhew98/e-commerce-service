using Api.Seedwork;
using CSharpFunctionalExtensions;
using Google.Apis.Auth.OAuth2.Requests;
using Google.Apis.Auth.OAuth2.Responses;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace Api.Infrastructure.Services
{
    public interface GoogleAuthService
    {
        Task<Result<TokenResponse, CommandErrorResponse>> RefreshAccessToken(RefreshTokenRequest googleAuthRequest);
    }

    public class GoogleServiceAccountCredential
    {
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("project_id")]
        public string ProjectId { get; set; }
        [JsonProperty("private_key_id")]
        public string PrivateKeyId { get; set; }
        [JsonProperty("private_key")]
        public string PrivateKey { get; set; }
        [JsonProperty("client_email")]
        public string ClientEmail { get; set; }
        [JsonProperty("client_id")]
        public string ClientId { get; set; }
        [JsonProperty("auth_uri")]
        public string AuthUri { get; set; }
        [JsonProperty("token_uri")]
        public string TokenUri { get; set; }
        [JsonProperty("auth_provider_x509_cert_url")]
        public string AuthProviderCertUrl { get; set; }
        [JsonProperty("client_x509_cert_url")]
        public string ClientCertUrl { get; set; }
    }

}
