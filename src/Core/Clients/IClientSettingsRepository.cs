using System.Threading.Tasks;

namespace Core.Clients
{
    public abstract class TraderSettingsBase
    {
        public abstract string GetId();


        public static T CreateDefault<T>() where T : TraderSettingsBase, new()
        {
            if (typeof (T) == typeof (KycProfileSettings))
                return KycProfileSettings.CreateDefault() as T;

            if (typeof(T) == typeof(RefundAddressSettings))
                return RefundAddressSettings.CreateDefault() as T;

            if (typeof(T) == typeof(ApplicationsSettings))
                return ApplicationsSettings.CreateDefault() as T;

            return new T();
        }
    }
  

    public class KycProfileSettings : TraderSettingsBase
    {
        public override string GetId()
        {
            return "KycProfile";
        }

        public bool ShowIdCard { get; set; }
        public bool ShowIdProofOfAddress { get; set; }
        public bool ShowSelfie { get; set; }

        public static KycProfileSettings CreateDefault()
        {
            return new KycProfileSettings
            {
                ShowIdCard = true,
                ShowIdProofOfAddress = true,
                ShowSelfie = true
            };
        }

    }

    public class RefundAddressSettings : TraderSettingsBase
    {
        public override string GetId()
        {
            return "RefundAddressSettings";
        }

        public string Address { get; set; }
        public int? ValidDays { get; set; }
        public bool? SendAutomatically { get; set; }

        public static RefundAddressSettings CreateDefault()
        {
            return new RefundAddressSettings
            {
                Address = string.Empty,
                ValidDays = 30,
                SendAutomatically = false
            };
        }
    }

    public class ApplicationsSettings : TraderSettingsBase
    {
        public override string GetId()
        {
            return "ApplicationsSettings";
        }

        public string[] TrustedApplicationIds { get; set; }

        public static ApplicationsSettings CreateDefault()
        {
            return new ApplicationsSettings
            {
                TrustedApplicationIds = new string[0]
            };
        }
    }

    public interface IClientSettingsRepository
    {
        Task<T> GetSettings<T>(string traderId) where T : TraderSettingsBase, new();
        Task SetSettings<T>(string traderId, T settings) where T : TraderSettingsBase, new();
        Task DeleteAsync<T>(string traderId) where T : TraderSettingsBase, new();

        Task UpdateKycDocumentSettingOnUpload(string clientId, string docType);
    }

}
