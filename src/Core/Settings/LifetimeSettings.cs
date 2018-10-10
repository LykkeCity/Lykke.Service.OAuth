using System;

namespace Core.Settings
{
    public class LifetimeSettings
    {
        public TimeSpan RefreshTokenLifetime { get; set; } = TimeSpan.FromDays(30);
        public TimeSpan AccessTokenLifetime { get; set; } = TimeSpan.FromMinutes(10);
        public TimeSpan MobileSessionLifetime { get; set; } = TimeSpan.FromDays(30);
        public TimeSpan RefreshTokenWhitelistLifetime { get; set; } = TimeSpan.FromDays(30);
        public TimeSpan SilentRefreshCookieLifetime { get; set; } = TimeSpan.FromDays(60);
        public TimeSpan SessionIdleTimeout { get; set; } = TimeSpan.FromMinutes(30);
    }
}
