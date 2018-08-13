using Lykke.SettingsReader.Attributes;

namespace WebAuth.Settings.ServiceSettings
{
    public class DbSettings
    {
        [AzureTableCheck]
        public string ClientPersonalInfoConnString { get; set; }
        [AzureTableCheck]
        public string LogsConnString { get; set; }
        [AzureBlobCheck]
        public string DataProtectionConnString { get; set; }
    }
}
