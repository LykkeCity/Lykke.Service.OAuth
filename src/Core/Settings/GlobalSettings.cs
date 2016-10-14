using System.Threading.Tasks;

namespace Core.Settings
{

    public static class GlobalSettings
    {
        public const int Mt4TimeOffset = 2;
    }


    public interface IAppGlobalSettings
    {
        string DepositUrl { get; }
        bool DebugMode { get; }
        string DefaultIosAssetGroup { get; set; }
        string DefaultAssetGroupForOther { get; set; }
    }

    public class AppGlobalSettings : IAppGlobalSettings
    {
        public string DepositUrl { get; set; }
        public bool DebugMode { get; set; }
        public string DefaultIosAssetGroup { get; set; }
        public string DefaultAssetGroupForOther { get; set; }
    }

    public interface IAppGlobalSettingsRepositry
    {
        Task SaveAsync(IAppGlobalSettings appGlobalSettings);
        Task<IAppGlobalSettings> GetAsync();
    }

}
