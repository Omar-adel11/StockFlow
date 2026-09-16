namespace Application.Services.Auth
{
    public class JWTSettings
    {
        public string key { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public int ExpirationInMinutes { get; set; }


    }
}