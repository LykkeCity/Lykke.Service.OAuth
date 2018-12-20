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
        public string DataProtectionConnString { get; set; }  
        [AzureBlobCheck]
        public string CertStorageConnectionString { get; set; }
        [AzureTableCheck]
        public string RegistrationUserStorageConnString { get; set; }    
        [AzureTableCheck]
        public string IroncladUserStorageConnString { get; set; }
    }
}
