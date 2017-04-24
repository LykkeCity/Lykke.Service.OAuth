using System.Threading.Tasks;
using AzureStorage;
using Core.Settings;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureDataAccess.Settings
{
    public class AppGlobalSettingsEntity : TableEntity, IAppGlobalSettings
    {
        public static string GeneratePartitionKey()
        {
            return "Setup";
        }

        public static string GenerateRowKey()
        {
            return "AppSettings";
        }


        public string DepositUrl { get; set; }
        public bool DebugMode { get; set; }
        public string DefaultIosAssetGroup { get; set; }
        public string DefaultAssetGroupForOther { get; set; }


        public static AppGlobalSettingsEntity Create(IAppGlobalSettings appGlobalSettings)
        {
            return new AppGlobalSettingsEntity
            {
                PartitionKey = GeneratePartitionKey(),
                RowKey = GenerateRowKey(),
                DepositUrl = appGlobalSettings.DepositUrl,
                DebugMode = appGlobalSettings.DebugMode,
                DefaultIosAssetGroup = appGlobalSettings.DefaultIosAssetGroup,
                DefaultAssetGroupForOther = appGlobalSettings.DefaultAssetGroupForOther
            };
        }
    }

    public class AppGlobalSettingsRepository : IAppGlobalSettingsRepositry
    {

        private readonly INoSQLTableStorage<AppGlobalSettingsEntity> _tableStorage;

        public AppGlobalSettingsRepository(INoSQLTableStorage<AppGlobalSettingsEntity> tableStorage)
        {
            _tableStorage = tableStorage;
        }

        public Task SaveAsync(IAppGlobalSettings appGlobalSettings)
        {
            var newEntity = AppGlobalSettingsEntity.Create(appGlobalSettings);
            return _tableStorage.InsertOrMergeAsync(newEntity);
        }

        public async Task<IAppGlobalSettings> GetAsync()
        {
            var partitionKey = AppGlobalSettingsEntity.GeneratePartitionKey();
            var rowKey = AppGlobalSettingsEntity.GenerateRowKey();
            return await _tableStorage.GetDataAsync(partitionKey, rowKey);
        }
    }
}
