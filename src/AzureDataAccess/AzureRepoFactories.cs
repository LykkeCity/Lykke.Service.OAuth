using AzureDataAccess.Application;
using AzureDataAccess.BackOffice;
using AzureDataAccess.Clients;
using AzureDataAccess.EventLogs;
using AzureDataAccess.Kyc;
using AzureStorage.Blob;
using AzureStorage.Tables;
using AzureStorage.Tables.Templates.Index;
using Common.Log;

namespace AzureDataAccess
{
    public static class AzureRepoFactories
    {
        public static ClientSettingsRepository CreateTraderSettingsRepository(string connString, ILog log)
        {
            return
                new ClientSettingsRepository(new AzureTableStorage<ClientSettingsEntity>(connString, "TraderSettings",
                    log));
        }

        public static class Clients
        {
            public static ClientsRepository CreateTradersRepository(string connstring, ILog log)
            {
                const string tableName = "Traders";
                return new ClientsRepository(
                    new AzureTableStorage<ClientAccountEntity>(connstring, tableName, log),
                    new AzureTableStorage<AzureIndex>(connstring, tableName, log));
            }

            public static PersonalDataRepository CreatePersonalDataRepository(string connString, ILog log)
            {
                return
                    new PersonalDataRepository(new AzureTableStorage<PersonalDataEntity>(connString, "PersonalData", log));
            }

            public static KycRepository CreateKycRepository(string connString, ILog log)
            {
                return new KycRepository(new AzureTableStorage<KycEntity>(connString, "KycStatuses", log));
            }

            public static KycDocumentsRepository CreateKycDocumentsRepository(string connString, ILog log)
            {
                return
                    new KycDocumentsRepository(new AzureTableStorage<KycDocumentEntity>(connString, "KycDocuments", log));
            }

            public static KycDocumentsScansRepository CreatKycDocumentsScansRepository(string connString)
            {
                return new KycDocumentsScansRepository(new AzureBlobStorage(connString));
            }

            public static ClientSessionsRepository CreateClientSessionsRepository(string connstring, ILog log)
            {
                return new ClientSessionsRepository(new AzureTableStorage<ClientSessionEntity>(connstring, "Sessions", log));
            }
        }

        public static class Applications
        {
            public static ApplicationRepository CreateApplicationsRepository(string connstring, ILog log)
            {
                const string tableName = "Applications";
                return new ApplicationRepository(new AzureTableStorage<ApplicationEntity>(connstring, tableName, log));
            }
        }

        public static class EventLogs
        {
            public static RegistrationLogs CreateRegistrationLogs(string connecionString, ILog log)
            {
                return
                    new RegistrationLogs(new AzureTableStorage<RegistrationLogEventEntity>(connecionString,
                        "LogRegistrations", log));
            }
        }

        public static class BackOffice
        {
            public static MenuBadgesRepository CreateMenuBadgesRepository(string connecionString, ILog log)
            {
                return
                    new MenuBadgesRepository(new AzureTableStorage<MenuBadgeEntity>(connecionString, "MenuBadges", log));
            }
        }
    }
}