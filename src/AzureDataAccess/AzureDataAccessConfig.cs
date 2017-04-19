using System;
using AzureDataAccess.AuditLog;
using AzureDataAccess.Email;
using AzureDataAccess.Log;
using AzureDataAccess.Partner;
using AzureDataAccess.Settings;
using AzureRepositories.Assets;
using AzureStorage.Queue;
using AzureStorage.Tables;
using Common.Log;
using Core.Application;
using Core.Assets.AssetGroup;
using Core.AuditLog;
using Core.Clients;
using Core.EventLogs;
using Core.Kyc;
using Core.Messages.Email;
using Core.Partner;
using Core.Settings;
using Core.Validation;
using StructureMap;

namespace AzureDataAccess
{
    public class AzureDataAccessConfig : Registry
    {
        public AzureDataAccessConfig(IBaseSettings settings)
        {
            var log = CreateLogToTable(settings.Db.LogsConnString);
            For<ILog>().Add(log);

            GeneralSettingsValidator.Validate(settings, log);

            var clientPersonalInfoConnString = settings.Db.ClientPersonalInfoConnString;

            BindLogs(clientPersonalInfoConnString, log);

            BindClients(clientPersonalInfoConnString, log);

            BindPartners(settings.Db.SharedStorageConnString, log);

            BindKyc(clientPersonalInfoConnString, log);

            BindApplications(clientPersonalInfoConnString, log);

            BindAssets(clientPersonalInfoConnString, log);

            BindSettings(clientPersonalInfoConnString, log);

            BindEmailMessages(clientPersonalInfoConnString);
        }

        private void BindPartners(string sharedStorageConnString, LogToTable log)
        {
            For<IPartnerAccountPolicyRepository>().Add
             (new PartnerAccountPolicyRepository(
                 new AzureTableStorage<PartnerAccountPolicyEntity>(sharedStorageConnString, "PartnerAccountPolicy", log)));

            For<IPartnerClientAccountRepository>().Add
            (new PartnerClientAccountRepository(
                new AzureTableStorage<PartnerClientAccountEntity>(sharedStorageConnString, "PartnerClientAccounts", log)));
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

            For<IPersonalDataRepository>().Add(
                AzureRepoFactories.Clients.CreatePersonalDataRepository(clientPersonalInfoConnString, log));
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
    }
}