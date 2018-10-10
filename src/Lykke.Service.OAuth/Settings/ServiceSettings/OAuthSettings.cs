using JetBrains.Annotations;
using Lykke.Service.OAuth.Settings;
using Core.Settings;
using Lykke.Service.OAuth.Settings.ServiceSettings;
using Lykke.SettingsReader.Attributes;

namespace WebAuth.Settings.ServiceSettings
{
    [UsedImplicitly]
    public class OAuthSettings
    {
        public DbSettings Db { get; set; }
        public CorsSettings Cors { get; set; } = new CorsSettings();
        [Optional]
        public CspSettings Csp { get; set; } = new CspSettings();
        public SecuritySettings Security { get; set; }
        public CacheSettings Cache { get; set; }
        public Certificates Certificates { get; set; }
        public CookieSettings CookieSettings { get; set; }
        public ResourceServerSettings ResourceServerSettings { get; set; }
        public RedisSettings RedisSettings { get; set; }
        public RegistrationProcessSettings RegistrationProcessSettings{get; set; }
        public LifetimeSettings LifetimeSettings { get; set; }
        public ExternalProvidersSettings ExternalProvidersSettings { get; set; }
    }
}
