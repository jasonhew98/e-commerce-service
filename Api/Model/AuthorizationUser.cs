using Newtonsoft.Json;

namespace Api.Model
{
    public class AuthorizationUser
    {
        public string AuthorizationUserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Username { get; set; }

        // Prevents password from being serialized and returned in API responses
        [JsonIgnore]
        public string Password { get; set; }
    }
}
