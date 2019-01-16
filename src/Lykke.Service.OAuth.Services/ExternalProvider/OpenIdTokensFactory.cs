using Core.ExternalProvider;
using Core.Settings;
using Microsoft.AspNetCore.Authentication;

namespace Lykke.Service.OAuth.Services.ExternalProvider
{
    public class OpenIdTokensFactory : IOpenIdTokensFactory
    {
        private readonly LifetimeSettings _lifetimeSettings;
        private readonly ISystemClock _clock;

        public OpenIdTokensFactory(
            ISystemClock clock,
            LifetimeSettings lifetimeSettings)
        {
            _clock = clock;
            _lifetimeSettings = lifetimeSettings;
        }

        public OpenIdTokens CreateOpenIdTokens(string idToken, string accessToken, string refreshToken)
        {
            var expiresAt = _clock.UtcNow.Add(_lifetimeSettings.IroncladAuthTokensLifetime);

            return new OpenIdTokens(idToken, accessToken, refreshToken, expiresAt);
        }
    }
}
