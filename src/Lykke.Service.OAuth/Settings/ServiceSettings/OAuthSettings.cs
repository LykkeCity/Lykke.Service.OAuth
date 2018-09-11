using JetBrains.Annotations;
using Lykke.SettingsReader.Attributes;
using StackExchange.Redis;

namespace WebAuth.Settings.ServiceSettings
{
    [UsedImplicitly]
    public class OAuthSettings
    {
        [HttpCheck("/api/isalive")]
        public string RegistrationApiUrl { get; set; }
        [HttpCheck("/api/isalive")]
        public string SessionApiUrl { get; set; }
        public DbSettings Db { get; set; }
        public CorsSettings Cors { get; set; } = new CorsSettings();
        [Optional]
        public CspSettings Csp { get; set; } = new CspSettings();
        public SecuritySettings Security { get; set; }
        public CacheSettings Cache { get; set; }
        public Certificates Certificates { get; set; }
        public CookieSettings CookieSettings { get; set; }
        public RedisSettings RedisSettings { get; set; }
    }

    public class RedisSettings
    {
        public string RedisConfiguration { get; set; }
    }
}
