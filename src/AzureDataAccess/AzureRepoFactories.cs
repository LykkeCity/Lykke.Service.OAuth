using AzureDataAccess.Application;
using AzureDataAccess.BackOffice;
using AzureDataAccess.Bitcoin;
using AzureDataAccess.Clients;
using AzureDataAccess.Kyc;
using AzureStorage.Blob;
using AzureStorage.Tables;
using AzureStorage.Tables.Templates.Index;
using Common.Log;
using Core.Bitcoin;
using Lykke.SettingsReader;

namespace AzureDataAccess
{
    public static class AzureRepoFactories
    {
        public static ClientSettingsRepository CreateTraderSettingsRepository(IReloadingManager<string> connString, ILog log)
        {
            return
                new ClientSettingsRepository(AzureTableStorage<ClientSettingsEntity>.Create(connString, "TraderSettings", log));
        }

        public static class Clients
        {
            public static ClientsRepository CreateTradersRepository(IReloadingManager<string> connstring, ILog log)
            {
                const string tableName = "Traders";
                return new ClientsRepository(
                    AzureTableStorage<ClientAccountEntity>.Create(connstring, tableName, log),
                    AzureTableStorage<AzureIndex>.Create(connstring, tableName, log));
            }

            public static KycRepository CreateKycRepository(IReloadingManager<string> connString, ILog log)
            {
                return new KycRepository(AzureTableStorage<KycEntity>.Create(connString, "KycStatuses", log));
            }

            public static KycDocumentsRepository CreateKycDocumentsRepository(IReloadingManager<string> connString, ILog log)
            {
                return
                    new KycDocumentsRepository(AzureTableStorage<KycDocumentEntity>.Create(connString, "KycDocuments", log));
            }

            public static KycDocumentsScansRepository CreatKycDocumentsScansRepository(IReloadingManager<string> connString)
            {
                return new KycDocumentsScansRepository(AzureBlobStorage.Create(connString));
            }

            public static ClientSessionsRepository CreateClientSessionsRepository(IReloadingManager<string> connstring, ILog log)
            {
                return new ClientSessionsRepository(AzureTableStorage<ClientSessionEntity>.Create(connstring, "Sessions", log));
            }
        }

        public static class Applications
        {
            public static ApplicationRepository CreateApplicationsRepository(IReloadingManager<string> connstring, ILog log)
            {
                const string tableName = "Applications";
                return new ApplicationRepository(AzureTableStorage<ApplicationEntity>.Create(connstring, tableName, log));
            }
        }

        public static class BackOffice
        {
            public static MenuBadgesRepository CreateMenuBadgesRepository(IReloadingManager<string> connecionString, ILog log)
            {
                return
                    new MenuBadgesRepository(AzureTableStorage<MenuBadgeEntity>.Create(connecionString, "MenuBadges", log));
            }
        }

        public static class Bitcoin
        {
            public static IWalletCredentialsRepository CreateWalletCredentialsRepository(IReloadingManager<string> connecionString, ILog log)
            {
                return new WalletCredentialsRepository(AzureTableStorage<WalletCredentialsEntity>.Create(connecionString,
                    "WalletCredentials", log));
            }
        }
    }
}
