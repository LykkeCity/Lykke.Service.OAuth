using Lykke.SettingsReader.Attributes;

namespace Core.Settings
{
    public class PersonalDataSettings
    {
        [HttpCheck("/api/isalive")]
        public string ServiceUri { get; set; }

        public string ApiKey { get; set; }
    }
}
