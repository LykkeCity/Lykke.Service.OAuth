﻿namespace Core.Settings
{
    public interface IOAuthSettings
    {
        LykkeServiceApiSettings LykkeServiceApi { get; set; }
        ServiceBusSettings EmailServiceBus { get; set; }
        OAuth OAuth { get; set; }
    }

    public class LykkeServiceApiSettings
    {
        public string ServiceUri { get; set; }
    }

    public class ServiceBusSettings
    {
        public string Key { get; set; }
        public string QueueName { get; set; }
        public string NamespaceUrl { get; set; }
        public string PolicyName { get; set; }
    }

    public class OAuth
    {
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
        public ServiceBusSettings EmailServiceBus { get; set; }
        public OAuth OAuth { get; set; }
    }
}