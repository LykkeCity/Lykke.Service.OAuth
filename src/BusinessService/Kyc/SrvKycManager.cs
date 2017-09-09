using System.Linq;
using System.Threading.Tasks;
using BusinessService.Infrastructure;
using Core.AuditLog;
using Core.BackOffice;
using Core.Clients;
using Core.Kyc;
using Lykke.Service.PersonalData.Contract;

namespace BusinessService.Kyc
{
    public class SrvKycManager : ISrvKycManager, IApplicationService
    {
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly IClientAccountsRepository _clientAccountsRepository;
        private readonly IClientSettingsRepository _clientSettingsRepository;
        private readonly IKycDocumentsRepository _kycDocumentsRepository;
        private readonly IKycDocumentsScansRepository _kycDocumentsScansRepository;
        private readonly IKycRepository _kycRepository;
        private readonly IMenuBadgesRepository _menuBadgesRepository;
        private readonly IPersonalDataService _personalDataService;

        public SrvKycManager(IKycDocumentsRepository kycDocumentsRepository,
            IKycDocumentsScansRepository kycDocumentsScansRepository,
            IKycRepository kycRepository,
            IPersonalDataService personalDataService, 
            IClientAccountsRepository clientAccountsRepository,
            IAuditLogRepository auditLogRepository,
            IClientSettingsRepository clientSettingsRepository,
            IMenuBadgesRepository menuBadgesRepository)
        {
            _kycDocumentsRepository = kycDocumentsRepository;
            _kycDocumentsScansRepository = kycDocumentsScansRepository;
            _kycRepository = kycRepository;
            _personalDataService = personalDataService;
            _clientAccountsRepository = clientAccountsRepository;
            _auditLogRepository = auditLogRepository;
            _clientSettingsRepository = clientSettingsRepository;
            _menuBadgesRepository = menuBadgesRepository;
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

        #endregion

        #region Clients

        public async Task ChangePhoneAsync(string clientId, string phoneNumber, string changer)
        {
            var dataBefore = await _personalDataService.GetAsync(clientId);
            await _clientAccountsRepository.ChangePhoneAsync(clientId, phoneNumber);
            await _personalDataService.ChangeContactPhoneAsync(clientId, phoneNumber);
            var dataAfter = await _personalDataService.GetAsync(clientId);

            await
                _auditLogRepository.AddAuditRecordAsync(clientId, dataBefore, dataAfter, AuditRecordType.PersonalData,
                    changer);
        }

        public async Task ChangeFirstNameAsync(string clientId, string firstName, string changer)
        {
            var dataBefore = await _personalDataService.GetAsync(clientId);
            await _personalDataService.ChangeFirstNameAsync(clientId, firstName);
            var dataAfter = await _personalDataService.GetAsync(clientId);

            await
                _auditLogRepository.AddAuditRecordAsync(clientId, dataBefore, dataAfter, AuditRecordType.PersonalData,
                    changer);
        }

        public async Task ChangeLastNameAsync(string clientId, string lastName, string changer)
        {
            var dataBefore = await _personalDataService.GetAsync(clientId);
            await _personalDataService.ChangeLastNameAsync(clientId, lastName);
            var dataAfter = await _personalDataService.GetAsync(clientId);

            await
                _auditLogRepository.AddAuditRecordAsync(clientId, dataBefore, dataAfter, AuditRecordType.PersonalData,
                    changer);
        }

        public async Task ChangeFullNameAsync(string clientId, string fullName, string changer)
        {
            var dataBefore = await _personalDataService.GetAsync(clientId);
            await _personalDataService.ChangeFullNameAsync(clientId, fullName);
            var dataAfter = await _personalDataService.GetAsync(clientId);

            await
                _auditLogRepository.AddAuditRecordAsync(clientId, dataBefore, dataAfter, AuditRecordType.PersonalData,
                    changer);
        }

        public async Task ChangeZipAsync(string clientId, string zip, string changer)
        {
            var dataBefore = await _personalDataService.GetAsync(clientId);
            await _personalDataService.ChangeZipAsync(clientId, zip);
            var dataAfter = await _personalDataService.GetAsync(clientId);

            await
                _auditLogRepository.AddAuditRecordAsync(clientId, dataBefore, dataAfter, AuditRecordType.PersonalData,
                    changer);
        }

        public async Task ChangeCityAsync(string clientId, string city, string changer)
        {
            var dataBefore = await _personalDataService.GetAsync(clientId);
            await _personalDataService.ChangeCityAsync(clientId, city);
            var dataAfter = await _personalDataService.GetAsync(clientId);

            await
                _auditLogRepository.AddAuditRecordAsync(clientId, dataBefore, dataAfter, AuditRecordType.PersonalData,
                    changer);
        }

        public async Task ChangeAddressAsync(string clientId, string address, string changer)
        {
            var dataBefore = await _personalDataService.GetAsync(clientId);
            await _personalDataService.ChangeAddressAsync(clientId, address);
            var dataAfter = await _personalDataService.GetAsync(clientId);

            await
                _auditLogRepository.AddAuditRecordAsync(clientId, dataBefore, dataAfter, AuditRecordType.PersonalData,
                    changer);
        }

        public async Task ChangeCountryAsync(string clientId, string country, string changer)
        {
            var dataBefore = await _personalDataService.GetAsync(clientId);
            await _personalDataService.ChangeCountryAsync(clientId, country);
            var dataAfter = await _personalDataService.GetAsync(clientId);

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