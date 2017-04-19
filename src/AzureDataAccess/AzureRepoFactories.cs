using AzureDataAccess.Application;
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
                const string clientPartnerRelationEntityTableName = "ClientPartnerRelations";
                return new ClientsRepository(
                    new AzureTableStorage<ClientAccountEntity>(connstring, tableName, log),
                    new AzureTableStorage<ClientPartnerRelationEntity>(connstring, clientPartnerRelationEntityTableName, log),
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
    }
}