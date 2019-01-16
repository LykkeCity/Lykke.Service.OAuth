using System;

namespace Core.Settings
{
    public class LifetimeSettings
    {
        public TimeSpan IroncladLoginSessionLifetime { get; set; }
        public TimeSpan IroncladAuthTokensLifetime { get; set; }
        public TimeSpan IroncladLogoutSessionLifetime { get; set; }
    }
}
