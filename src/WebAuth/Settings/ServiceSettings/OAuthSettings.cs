using Lykke.SettingsReader.Attributes;

namespace WebAuth.Settings.ServiceSettings
{
    public class OAuthSettings
    {
        public DbSettings Db { get; set; }
        public CorsSettings Cors { get; set; } = new CorsSettings();
        [Optional]
        public CspSettings Csp { get; set; } = new CspSettings();
        public SecuritySettings Security { get; set; }
        public CacheSettings Cache { get; set; }
    }
}
