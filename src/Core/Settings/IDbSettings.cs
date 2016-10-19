namespace Core.Settings
{
    public class DbSettings
    {
        public string ClientPersonalInfoConnString { get; set; }
        public string LogsConnString { get; set; }
    }

    public class LykkeServiceApiSettings
    {
        public string ServiceUri { get; set; }
    }

    public class BaseSettings
    {
        public DbSettings Db { get; set; }

        public LykkeServiceApiSettings LykkeServiceApi { get; set; }
    }

}
