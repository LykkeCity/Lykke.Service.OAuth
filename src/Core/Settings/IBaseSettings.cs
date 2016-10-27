using System.ComponentModel.DataAnnotations;

namespace Core.Settings
{
    public interface IBaseSettings
    {
        DbSettings Db { get; set; }

        LykkeServiceApiSettings LykkeServiceApi { get; set; }

        bool IsDebug { get; set; }
    }

    public class DbSettings
    {
        [Required]
        public string ClientPersonalInfoConnString { get; set; }

        [Required]
        public string LogsConnString { get; set; }
    }

    public class LykkeServiceApiSettings
    {
        [Required]
        public string ServiceUri { get; set; }
    }

    public class BaseSettings : IBaseSettings
    {
        [Required]
        public DbSettings Db { get; set; }

        [Required]
        public LykkeServiceApiSettings LykkeServiceApi { get; set; }

        public bool IsDebug { get; set; }
    }
}