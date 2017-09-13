namespace Core.Settings
{
    public interface IOAuthSettings
    {
        LykkeServiceApiSettings LykkeServiceApi { get; set; }
        OAuth OAuth { get; set; }
        PersonalDataSettings PersonalDataServiceSettings { get; set; }
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
        public CorsSettings Cors { get; set; }        
    }

    public class DbSettings
    {
        public string ClientPersonalInfoConnString { get; set; }
        public string LogsConnString { get; set; }
        public string BackOfficeConnString { get; set; }
    }

    public class CorsSettings
    {
        public string[] Origins { get; set; }
    }

    public class OAuthSettings : IOAuthSettings
    {
        public LykkeServiceApiSettings LykkeServiceApi { get; set; }
        public OAuth OAuth { get; set; }
        public PersonalDataSettings PersonalDataServiceSettings { get; set; }
        public SlackNotificationsSettings SlackNotifications { get; set; }
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