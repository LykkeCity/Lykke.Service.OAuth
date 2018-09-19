using AzureDataAccess.Application;
using AzureDataAccess.Bitcoin;
using AzureStorage.Tables;
using Common.Log;
using Core.Bitcoin;
using Lykke.SettingsReader;

namespace AzureDataAccess
{
    public static class AzureRepoFactories
    {
        public static ApplicationRepository CreateApplicationsRepository(IReloadingManager<string> connstring, ILog log)
        {
            const string tableName = "Applications";
            return new ApplicationRepository(AzureTableStorage<ApplicationEntity>.Create(connstring, tableName, log));
        }

        public static IWalletCredentialsRepository CreateWalletCredentialsRepository(IReloadingManager<string> connecionString, ILog log)
        {
            return new WalletCredentialsRepository(AzureTableStorage<WalletCredentialsEntity>.Create(connecionString,
                "WalletCredentials", log));
        }
    }
}
