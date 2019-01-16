using System;

namespace Core.ExternalProvider
{
    public class OpenIdTokens
    {
        public string IdToken { get; }

        public string AccessToken { get; }

        public string RefreshToken { get; }

        public DateTimeOffset ExpiresAt { get; }

        public OpenIdTokens(string idToken, string accessToken, string refreshToken, DateTimeOffset expiresAt)
        {
            IdToken = idToken;
            AccessToken = accessToken;
            RefreshToken = refreshToken;
            ExpiresAt = expiresAt;
        }
    }
}
