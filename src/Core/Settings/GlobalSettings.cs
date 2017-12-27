using System.Threading.Tasks;

namespace Core.Settings
{
    public interface IAppGlobalSettings
    {
        string DepositUrl { get; }
        bool DebugMode { get; }
        string DefaultIosAssetGroup { get; set; }
        string DefaultAssetGroupForOther { get; set; }
    }

    public interface IAppGlobalSettingsRepositry
    {
        Task SaveAsync(IAppGlobalSettings appGlobalSettings);
        Task<IAppGlobalSettings> GetAsync();
    }
}
