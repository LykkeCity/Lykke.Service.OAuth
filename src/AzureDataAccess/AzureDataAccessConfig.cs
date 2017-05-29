using AzureDataAccess.AuditLog;
using AzureDataAccess.Email;
using AzureDataAccess.Log;
using AzureDataAccess.Settings;
using AzureRepositories.Assets;
using AzureStorage.Queue;
using AzureStorage.Tables;
using Common.Log;
using Core.Application;
using Core.Assets.AssetGroup;
using Core.AuditLog;
using Core.BackOffice;
using Core.Bitcoin;
using Core.Clients;
using Core.EventLogs;
using Core.Kyc;
using Core.Messages.Email;
using Core.Settings;
using Core.UserProfile;
using StructureMap;

namespace AzureDataAccess
{
    public class AzureDataAccessConfig : Registry
    {
        public AzureDataAccessConfig(IOAuthSettings settings)
        {
            var log = CreateLogToTable(settings.OAuth.Db.LogsConnString);
            For<ILog>().Add(log);

            var clientPersonalInfoConnString = settings.OAuth.Db.ClientPersonalInfoConnString;
            var backOfficeConnString = settings.OAuth.Db.BackOfficeConnString;

            BindLogs(clientPersonalInfoConnString, log);

            BindClients(clientPersonalInfoConnString, log);

            BindKyc(clientPersonalInfoConnString, log);

            BindApplications(clientPersonalInfoConnString, log);

            BindAssets(clientPersonalInfoConnString, log);

            BindSettings(clientPersonalInfoConnString, log);

            BindEmailMessages(clientPersonalInfoConnString);

            BindBackOffice(backOfficeConnString, log);

            BindBitcoin(clientPersonalInfoConnString, log);

            BindUserProfile(clientPersonalInfoConnString, log);
        }

        public static LogToTable CreateLogToTable(string connString)
        {
            return new LogToTable(new AzureTableStorage<LogEntity>(connString, "LogWebAuth", null));
        }

        private void BindEmailMessages(string clientPersonalInfoConnString)
        {
            For<IEmailCommandProducer>().Add(
                new EmailCommandProducer(new AzureQueueExt(clientPersonalInfoConnString, "emailsqueue")));
        }

        private void BindSettings(string clientPersonalInfoConnString, ILog log)
        {
            For<IAppGlobalSettingsRepositry>().Add(new AppGlobalSettingsRepository(
                new AzureTableStorage<AppGlobalSettingsEntity>(clientPersonalInfoConnString, "Setup", log)));
        }

        private void BindAssets(string clientPersonalInfoConnString, ILog log)
        {
            For<IAssetGroupRepository>().Add(
                new AssetGroupRepository(
                    new AzureTableStorage<AssetGroupEntity>(clientPersonalInfoConnString, "AssetGroups", log)));
        }

        private void BindApplications(string clientPersonalInfoConnString, ILog log)
        {
            For<IApplicationRepository>().Add(
                AzureRepoFactories.Applications.CreateApplicationsRepository(clientPersonalInfoConnString,
                    log));
        }

        private void BindKyc(string clientPersonalInfoConnString, ILog log)
        {
            For<IKycRepository>().Add(
                AzureRepoFactories.Clients.CreateKycRepository(clientPersonalInfoConnString, log));

            For<IKycDocumentsRepository>().Add(
                AzureRepoFactories.Clients.CreateKycDocumentsRepository(clientPersonalInfoConnString, log));

            For<IKycDocumentsScansRepository>().Add(
                AzureRepoFactories.Clients.CreatKycDocumentsScansRepository(clientPersonalInfoConnString));
        }

        private void BindClients(string clientPersonalInfoConnString, ILog log)
        {
            For<IClientAccountsRepository>().Add(
                AzureRepoFactories.Clients.CreateTradersRepository(clientPersonalInfoConnString, log));

            For<IClientsSessionsRepository>().Add(
                AzureRepoFactories.Clients.CreateClientSessionsRepository(clientPersonalInfoConnString, log));
        }

        private void BindLogs(string clientPersonalInfoConnString, ILog log)
        {
            For<IRegistrationLogs>().Add(
                AzureRepoFactories.EventLogs.CreateRegistrationLogs(clientPersonalInfoConnString, log));

            For<IAuditLogRepository>().Add(
                new AuditLogRepository(
                    new AzureTableStorage<AuditLogDataEntity>(clientPersonalInfoConnString, "AuditLogs", log)));

            For<IClientSettingsRepository>().Add(
                AzureRepoFactories.CreateTraderSettingsRepository(clientPersonalInfoConnString, log));
        }

        private void BindBackOffice(string backOfficeConnString, ILog log)
        {
            For<IMenuBadgesRepository>().Add(
                AzureRepoFactories.BackOffice.CreateMenuBadgesRepository(backOfficeConnString, log));
        }

        private void BindBitcoin(string clientPersonalInfoConnString, ILog log)
        {
            For<IWalletCredentialsRepository>().Add(
                AzureRepoFactories.Bitcoin.CreateWalletCredentialsRepository(clientPersonalInfoConnString, log));
        }

        private void BindUserProfile(string clientPersonalInfoConnString, ILog log)
        {
            For<IUserProfileRepository>().Add(
                AzureRepoFactories.UserProfile.CreateUserProfileRepository(clientPersonalInfoConnString, log));
        }
    }
}