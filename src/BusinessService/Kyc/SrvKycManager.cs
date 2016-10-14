using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BusinessService.Infrastructure;
using Core.Assets.AssetGroup;
using Core.AuditLog;
using Core.BackOffice;
using Core.Clients;
using Core.EventLogs;
using Core.Kyc;
using Core.Messages;
using Core.Settings;

namespace BusinessService.Kyc
{
    public class SrvKycManager : ISrvKycManager, IApplicationService
    {
        private readonly IAppGlobalSettingsRepositry _appGlobalSettingsRepositry;
        private readonly IAssetGroupRepository _assetGroupRepository;
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly IClientAccountsRepository _clientAccountsRepository;
        private readonly IClientSettingsRepository _clientSettingsRepository;
        private readonly IKycDocumentsRepository _kycDocumentsRepository;
        private readonly IKycDocumentsScansRepository _kycDocumentsScansRepository;
        private readonly IKycRepository _kycRepository;
        private readonly IMenuBadgesRepository _menuBadgesRepository;
        private readonly IPersonalDataRepository _personalDataRepository;
        private readonly IRegistrationConsumer[] _registrationConsumers;
        private readonly IRegistrationLogs _registrationLogs;
        private readonly ISrvEmailsFacade _srvEmailsFacade;

        public SrvKycManager(IKycDocumentsRepository kycDocumentsRepository,
            IKycDocumentsScansRepository kycDocumentsScansRepository,
            IKycRepository kycRepository,
            IPersonalDataRepository personalDataRepository, IClientAccountsRepository clientAccountsRepository,
            IRegistrationConsumer[] registrationConsumers, IAuditLogRepository auditLogRepository,
            IRegistrationLogs registrationLogs, IClientSettingsRepository clientSettingsRepository,
            IAppGlobalSettingsRepositry appGlobalSettingsRepositry, IAssetGroupRepository assetGroupRepository,
            ISrvEmailsFacade srvEmailsFacade)
        {
            _kycDocumentsRepository = kycDocumentsRepository;
            _kycDocumentsScansRepository = kycDocumentsScansRepository;
            _kycRepository = kycRepository;
            _personalDataRepository = personalDataRepository;
            _clientAccountsRepository = clientAccountsRepository;
            _registrationConsumers = registrationConsumers;
            _auditLogRepository = auditLogRepository;
            _registrationLogs = registrationLogs;
            _clientSettingsRepository = clientSettingsRepository;
            _appGlobalSettingsRepositry = appGlobalSettingsRepositry;
            _assetGroupRepository = assetGroupRepository;
            _srvEmailsFacade = srvEmailsFacade;
        }

        #region Documents

        public async Task<string> UploadDocument(string clientId, string type, string fileName, string mime, byte[] data,
            string changer)
        {
            var documentBeforeTask = _kycDocumentsRepository.GetAsync(clientId);
            var kycDocument = await _kycDocumentsRepository.AddAsync(KycDocument.Create(clientId, type, mime, fileName));
            await _kycDocumentsScansRepository.AddDocument(kycDocument.DocumentId, data);

            await UpdateKycProfileSettings(clientId);

            var documentBefore = (await documentBeforeTask)?
                .OrderByDescending(x => x.DateTime)
                .FirstOrDefault(x => x.Type == type);

            await _auditLogRepository.AddAuditRecordAsync(clientId, documentBefore, kycDocument,
                AuditRecordType.KycDocument, changer);

            return kycDocument.DocumentId;
        }

        public async Task<IKycDocument> DeleteAsync(string clientId, string documentId, string changer)
        {
            var document = await _kycDocumentsRepository.DeleteAsync(clientId, documentId);

            await UpdateKycProfileSettings(clientId);

            await
                _auditLogRepository.AddAuditRecordAsync(clientId, document, null, AuditRecordType.KycDocument, changer);

            return document;
        }

        private async Task UpdateKycProfileSettings(string clientId)
        {
            var documents = (await _kycDocumentsRepository.GetAsync(clientId)).ToArray();

            var settings = await _clientSettingsRepository.GetSettings<KycProfileSettings>(clientId);

            settings.ShowIdCard = !documents.HasType(KycDocumentTypes.IdCard);
            settings.ShowIdProofOfAddress = !documents.HasType(KycDocumentTypes.ProofOfAddress);
            settings.ShowSelfie = !documents.HasType(KycDocumentTypes.Selfie);

            await _clientSettingsRepository.SetSettings(clientId, settings);
        }

        #endregion

        #region KYC Status

        private async Task UpdateKycBadge()
        {
            var count = (await _kycRepository.GetClientsByStatus(KycStatus.Pending)).Count();
            await _menuBadgesRepository.SaveBadgeAsync(MenuBadges.Kyc, count.ToString());
        }

        public async Task<bool> ChangeKycStatus(string clientId, KycStatus kycStatus, string changer)
        {
            var currentStatus = await _kycRepository.GetKycStatusAsync(clientId);

            if (currentStatus != kycStatus)
            {
                await _kycRepository.SetStatusAsync(clientId, kycStatus);
                await _auditLogRepository.AddAuditRecordAsync(clientId, currentStatus, kycStatus,
                    AuditRecordType.KycStatus, changer);
                await UpdateKycBadge();
                return true;
            }

            return false;
        }


        public async Task<IEnumerable<IPersonalData>> GetAccountsToCheck()
        {
            var ids = await _kycRepository.GetClientsByStatus(KycStatus.Pending);
            return await _personalDataRepository.GetAsync(ids);
        }

        #endregion

        #region Clients

        public async Task<IClientAccount> RegisterClientAsync(string email, string firstName, string lastName,
            string phone, string password, string hint, string clientInfo, string ip, string changer, string language)
        {
            IClientAccount clientAccount = ClientAccount.Create(email, phone);

            clientAccount = await _clientAccountsRepository.RegisterAsync(clientAccount, password);

            var personalData = FullPersonalData.Create(clientAccount, firstName, lastName, hint);
            await _personalDataRepository.SaveAsync(personalData);

            await SetDefaultAssetGroups(clientAccount.Id);

            var fullname = personalData.GetFullName();

            var logEvent = RegistrationLogEvent.Create(clientAccount.Id, email, fullname, phone, clientInfo, ip);
            await _registrationLogs.RegisterEventAsync(logEvent);

            await _auditLogRepository.AddAuditRecordAsync(clientAccount.Id, null, personalData,
                AuditRecordType.PersonalData, changer);

            await _srvEmailsFacade.SendWelcomeEmail(clientAccount.Email, clientAccount.Id);

            foreach (var registrationConsumer in _registrationConsumers)
                registrationConsumer.ConsumeRegistration(clientAccount, ip, language);

            return clientAccount;
        }

        private async Task SetDefaultAssetGroups(string clientId)
        {
            var globalSettings = await _appGlobalSettingsRepositry.GetAsync();
            if (!string.IsNullOrEmpty(globalSettings.DefaultAssetGroupForOther))
                await _assetGroupRepository.AddClientToGroup(clientId, globalSettings.DefaultAssetGroupForOther);
        }

        public async Task UpdatePersonalDataAsync(IPersonalData personalData, string changer)
        {
            var dataBefore = await _personalDataRepository.GetAsync(personalData.Id);
            await _personalDataRepository.UpdateAsync(personalData);
            var dataAfter = await _personalDataRepository.GetAsync(personalData.Id);

            await _auditLogRepository.AddAuditRecordAsync(personalData.Id, dataBefore, dataAfter,
                AuditRecordType.PersonalData, changer);
        }

        public async Task ChangePhoneAsync(string clientId, string phoneNumber, string changer)
        {
            var dataBefore = await _personalDataRepository.GetAsync(clientId);
            await _clientAccountsRepository.ChangePhoneAsync(clientId, phoneNumber);
            await _personalDataRepository.ChangeContactPhoneAsync(clientId, phoneNumber);
            var dataAfter = await _personalDataRepository.GetAsync(clientId);

            await
                _auditLogRepository.AddAuditRecordAsync(clientId, dataBefore, dataAfter, AuditRecordType.PersonalData,
                    changer);
        }

        public async Task ChangeFullNameAsync(string clientId, string fullName, string changer)
        {
            var dataBefore = await _personalDataRepository.GetAsync(clientId);
            await _personalDataRepository.ChangeFullNameAsync(clientId, fullName);
            var dataAfter = await _personalDataRepository.GetAsync(clientId);

            await
                _auditLogRepository.AddAuditRecordAsync(clientId, dataBefore, dataAfter, AuditRecordType.PersonalData,
                    changer);
        }

        public async Task ChangeFirstNameAsync(string clientId, string firstName, string changer)
        {
            var dataBefore = await _personalDataRepository.GetAsync(clientId);
            await _personalDataRepository.ChangeFirstNameAsync(clientId, firstName);
            var dataAfter = await _personalDataRepository.GetAsync(clientId);

            await
                _auditLogRepository.AddAuditRecordAsync(clientId, dataBefore, dataAfter, AuditRecordType.PersonalData,
                    changer);
        }

        public async Task ChangeLastNameAsync(string clientId, string lastName, string changer)
        {
            var dataBefore = await _personalDataRepository.GetAsync(clientId);
            await _personalDataRepository.ChangeLastNameAsync(clientId, lastName);
            var dataAfter = await _personalDataRepository.GetAsync(clientId);

            await
                _auditLogRepository.AddAuditRecordAsync(clientId, dataBefore, dataAfter, AuditRecordType.PersonalData,
                    changer);
        }

        public async Task ChangeZipAsync(string clientId, string zip, string changer)
        {
            var dataBefore = await _personalDataRepository.GetAsync(clientId);
            await _personalDataRepository.ChangeZipAsync(clientId, zip);
            var dataAfter = await _personalDataRepository.GetAsync(clientId);

            await
                _auditLogRepository.AddAuditRecordAsync(clientId, dataBefore, dataAfter, AuditRecordType.PersonalData,
                    changer);
        }

        public async Task ChangeCityAsync(string clientId, string city, string changer)
        {
            var dataBefore = await _personalDataRepository.GetAsync(clientId);
            await _personalDataRepository.ChangeCityAsync(clientId, city);
            var dataAfter = await _personalDataRepository.GetAsync(clientId);

            await
                _auditLogRepository.AddAuditRecordAsync(clientId, dataBefore, dataAfter, AuditRecordType.PersonalData,
                    changer);
        }

        public async Task ChangeAddressAsync(string clientId, string address, string changer)
        {
            var dataBefore = await _personalDataRepository.GetAsync(clientId);
            await _personalDataRepository.ChangeAddressAsync(clientId, address);
            var dataAfter = await _personalDataRepository.GetAsync(clientId);

            await
                _auditLogRepository.AddAuditRecordAsync(clientId, dataBefore, dataAfter, AuditRecordType.PersonalData,
                    changer);
        }

        public async Task ChangeCountryAsync(string clientId, string country, string changer)
        {
            var dataBefore = await _personalDataRepository.GetAsync(clientId);
            await _personalDataRepository.ChangeCountryAsync(clientId, country);
            var dataAfter = await _personalDataRepository.GetAsync(clientId);

            await
                _auditLogRepository.AddAuditRecordAsync(clientId, dataBefore, dataAfter, AuditRecordType.PersonalData,
                    changer);
        }

        #endregion
    }

    public static class RecordChanger
    {
        public const string Client = "Client";
    }
}