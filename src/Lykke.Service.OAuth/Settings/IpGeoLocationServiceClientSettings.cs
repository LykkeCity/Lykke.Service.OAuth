using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.OAuth.Settings
{
    public class IpGeoLocationServiceClientSettings
    {
        [HttpCheck("/api/isalive")]
        public string ServiceUrl { get; set; }
    }
}
