using System;
using Core.Services;
using Core.Settings;
using JetBrains.Annotations;

namespace Lykke.Service.OAuth.Services
{
    /// <inheritdoc/>
    public class LifetimeSettingsProvider : ILifetimeSettingsProvider
    {
        private readonly LifetimeSettings _lifetimeSettings;

        public LifetimeSettingsProvider(
            [NotNull]LifetimeSettings lifetimeSettings)
        {
            _lifetimeSettings = lifetimeSettings;
        }

        public TimeSpan GetRefreshTokenLifetime()
        {
            return _lifetimeSettings.RefreshTokenLifetime;
        }

        public TimeSpan GetAccessTokenLifetime()
        {
            return _lifetimeSettings.AccessTokenLifetime;
        }

        public TimeSpan GetMobileSessionLifetime()
        {
            return _lifetimeSettings.MobileSessionLifetime;
        }

        public TimeSpan GetRefreshTokenWhitelistLifetime()
        {
            return _lifetimeSettings.RefreshTokenWhitelistLifetime;
        }

        public TimeSpan GetSilentRefreshCookieLifetime()
        {
            return _lifetimeSettings.SilentRefreshCookieLifetime;
        }
    }
}
