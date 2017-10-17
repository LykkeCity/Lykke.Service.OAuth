using System;
using Lykke.SettingsReader.Attributes;

namespace Core.Settings
{
    public class OAuthSettings
    {
        public LykkeServiceApiSettings LykkeServiceApi { get; set; }
        public OAuth OAuth { get; set; }
        public PersonalDataSettings PersonalDataServiceSettings { get; set; }
        public SlackNotificationsSettings SlackNotifications { get; set; }
    }

    public class LykkeServiceApiSettings
    {
        public string ServiceUri { get; set; }
    }

    public class OAuth
    {
        public string RegistrationApiUrl { get; set; }
        public string SessionApiUrl { get; set; }
        public DbSettings Db { get; set; }
        public CorsSettings Cors { get; set; } = new CorsSettings();
        [Optional]
        public CspSettings Csp { get; set; } = new CspSettings();
    }

    public class DbSettings
    {
        public string ClientPersonalInfoConnString { get; set; }
        public string LogsConnString { get; set; }
        public string BackOfficeConnString { get; set; }
    }

    public class CorsSettings
    {
        public string[] Origins { get; set; } = Array.Empty<string>();
    }

    public class CspSettings
    {
        [Optional]
        public string[] ScriptSources { get; set; } = Array.Empty<string>();
        [Optional]
        public string[] StyleSources { get; set; } = Array.Empty<string>();
        [Optional]
        public string[] FontSources { get; set; } = Array.Empty<string>();
    }

    public class CspSettings
    {
        [Optional]
        public string[] ScriptSources { get; set; } = Array.Empty<string>();
        [Optional]
        public string[] StyleSources { get; set; } = Array.Empty<string>();
        [Optional]
        public string[] FontSources { get; set; } = Array.Empty<string>();
    }

    public class SlackNotificationsSettings
    {
        public AzureQueueSettings AzureQueue { get; set; }
        public int ThrottlingLimitSeconds { get; set; }
    }

    public class AzureQueueSettings
    {
        public string ConnectionString { get; set; }
        public string QueueName { get; set; }
    }
}