using System.ComponentModel.DataAnnotations;

namespace Core.Settings
{
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

    public class BaseSettings
    {
        [Required]
        public DbSettings Db { get; set; }

        [Required]
        public LykkeServiceApiSettings LykkeServiceApi { get; set; }

        public bool IsDebug { get; set; }
    }

}
