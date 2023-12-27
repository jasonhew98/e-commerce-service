using Newtonsoft.Json.Linq;

namespace Api.Model
{
    public class Credential
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public JObject ServiceAccount { get; set; }

        public bool HasServiceAccount()
        {
            return ServiceAccount.HasValues;
        }
    }
}
