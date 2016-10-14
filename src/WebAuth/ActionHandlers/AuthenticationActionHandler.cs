using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using BusinessService.Kyc;
using Core.Clients;
using Core.Kyc;
using Microsoft.AspNetCore.Http;
using WebAuth.Models;

namespace WebAuth.ActionHandlers
{
    public class AuthenticationActionHandler
    {
        private readonly IClientSettingsRepository _clientSettingsRepository;
        private readonly IKycDocumentsRepository _kycDocumentsRepository;
        private readonly IKycRepository _kycRepository;
        private readonly ISrvKycManager _srvKycManager;

        public AuthenticationActionHandler(IKycRepository kycRepository, ISrvKycManager srvKycManager,
            IClientSettingsRepository clientSettingsRepository, IKycDocumentsRepository kycDocumentsRepository)
        {
            _kycRepository = kycRepository;
            _srvKycManager = srvKycManager;
            _clientSettingsRepository = clientSettingsRepository;
            _kycDocumentsRepository = kycDocumentsRepository;
        }

        public async Task UploadFileAsync(DocumentViewModel documentModel, string clientId)
        {
            if ((documentModel.Document != null) && (documentModel.Document.Length > 0))
            {
                var kycStatus = await UploadDocumentAsync(clientId, documentModel.Document, documentModel.DocumentType);

                await UpdateKycStatusAsync(clientId, kycStatus);
            }
        }

        public async Task<KycStatus> UploadDocumentAsync(string clientId, IFormFile document, string type)
        {
            var status = await _kycRepository.GetKycStatusAsync(clientId);

            if (status != KycStatus.NeedToFillData)
                return status;

            var parsedContentDisposition = ContentDispositionHeaderValue.Parse(document.ContentDisposition);
            var fileName = parsedContentDisposition.FileName.Trim('"');

            var mimeType = document.ContentType;

            using (var fileStream = document.OpenReadStream())
            using (var ms = new MemoryStream())
            {
                fileStream.CopyTo(ms);
                var fileBytes = ms.ToArray();

                await
                    _srvKycManager.UploadDocument(clientId, type, fileName, mimeType, fileBytes, RecordChanger.Client);
            }

            await _clientSettingsRepository.UpdateKycDocumentSettingOnUpload(clientId, type);

            return status;
        }

        public async Task UpdateKycStatusAsync(string clientId, KycStatus status)
        {
            var documents = (await _kycDocumentsRepository.GetAsync(clientId)).ToArray();

            if ((status == KycStatus.NeedToFillData) && documents.HasAllTypes())
                await _srvKycManager.ChangeKycStatus(clientId, KycStatus.Pending, RecordChanger.Client);
        }
    }
}