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

namespace AzureDataAccess
{
    public static class AzureRepoFactories
    {
        public static ClientSettingsRepository CreateTraderSettingsRepository(string connString, ILog log)
        {
            return
                new ClientSettingsRepository(AzureTableStorage<ClientSettingsEntity>.Create(() => connString, "TraderSettings",
                    log));
        }

        public static class Clients
        {
            public static ClientsRepository CreateTradersRepository(string connstring, ILog log)
            {
                const string tableName = "Traders";
                return new ClientsRepository(
                    AzureTableStorage<ClientAccountEntity>.Create(() => connstring, tableName, log),
                    AzureTableStorage<AzureIndex>.Create(() => connstring, tableName, log));
            }

            public static KycRepository CreateKycRepository(string connString, ILog log)
            {
                return new KycRepository(AzureTableStorage<KycEntity>.Create(() => connString, "KycStatuses", log));
            }

            public static KycDocumentsRepository CreateKycDocumentsRepository(string connString, ILog log)
            {
                return
                    new KycDocumentsRepository(AzureTableStorage<KycDocumentEntity>.Create(() => connString, "KycDocuments", log));
            }

            public static KycDocumentsScansRepository CreatKycDocumentsScansRepository(string connString)
            {
                return new KycDocumentsScansRepository(new AzureBlobStorage(connString));
            }            
        }

        public static class Applications
        {
            public static ApplicationRepository CreateApplicationsRepository(string connstring, ILog log)
            {
                const string tableName = "Applications";
                return new ApplicationRepository(AzureTableStorage<ApplicationEntity>.Create(() => connstring, tableName, log));
            }
        }

        public static class BackOffice
        {
            public static MenuBadgesRepository CreateMenuBadgesRepository(string connecionString, ILog log)
            {
                return
                    new MenuBadgesRepository(AzureTableStorage<MenuBadgeEntity>.Create(() => connecionString, "MenuBadges", log));
            }
        }

        public static class Bitcoin
        {
            public static IWalletCredentialsRepository CreateWalletCredentialsRepository(string connecionString, ILog log)
            {
                return new WalletCredentialsRepository(AzureTableStorage<WalletCredentialsEntity>.Create(() => connecionString,
                    "WalletCredentials", log));
            }
        }
    }
}