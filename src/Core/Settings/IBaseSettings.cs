using System.ComponentModel.DataAnnotations;

namespace Core.Settings
{
    public interface IBaseSettings
    {
        DbSettings Db { get; set; }

        LykkeServiceApiSettings LykkeServiceApi { get; set; }

        ServiceBusSettings EmailServiceBus { get; set; }

        bool IsDebug { get; set; }
    }

    public class DbSettings
    {
        [Required]
        public string ClientPersonalInfoConnString { get; set; }

        [Required]
        public string SharedStorageConnString { get; set; }

        [Required]
        public string LogsConnString { get; set; }
    }

    public class LykkeServiceApiSettings
    {
        [Required]
        public string ServiceUri { get; set; }
    }

    public class ServiceBusSettings
    {
        public string Key { get; set; }
        public string QueueName { get; set; }
        public string NamespaceUrl { get; set; }
        public string PolicyName { get; set; }
    }

    public class BaseSettings : IBaseSettings
    {
        [Required]
        public DbSettings Db { get; set; }

        [Required]
        public LykkeServiceApiSettings LykkeServiceApi { get; set; }

        public ServiceBusSettings EmailServiceBus { get; set; }

        public bool IsDebug { get; set; }
    }
}