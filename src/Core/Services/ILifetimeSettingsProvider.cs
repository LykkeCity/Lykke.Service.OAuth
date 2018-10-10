using System;

namespace Core.Services
{
    /// <summary>
    ///     Provider for lifetime settings.
    /// </summary>
    public interface ILifetimeSettingsProvider
    {
        TimeSpan GetRefreshTokenLifetime();
        TimeSpan GetAccessTokenLifetime();
        TimeSpan GetMobileSessionLifetime();
        TimeSpan GetRefreshTokenWhitelistLifetime();
        TimeSpan GetSilentRefreshCookieLifetime();
    }
}
