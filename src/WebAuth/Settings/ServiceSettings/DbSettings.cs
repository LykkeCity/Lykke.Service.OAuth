using JetBrains.Annotations;
using Lykke.SettingsReader.Attributes;

namespace WebAuth.Settings.ServiceSettings
{
    [UsedImplicitly]
    public class DbSettings
    {
        [AzureTableCheck]
        public string ClientPersonalInfoConnString { get; set; }
        [AzureTableCheck]
        public string LogsConnString { get; set; }
        [AzureBlobCheck]
        public string CertStorageConnString { get; set; }
    }
}
