using Lykke.SettingsReader.Attributes;

namespace WebAuth.Settings.ClientSettings
{
    public class ClientAccountSettings
    {
        [HttpCheck("/api/isalive")]
        public string ServiceUrl { get; set; }
    }
}
