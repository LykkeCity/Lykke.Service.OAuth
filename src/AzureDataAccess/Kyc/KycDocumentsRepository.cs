using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage;
using Core.Kyc;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureDataAccess.Kyc
{
    public class KycDocumentEntity : TableEntity, IKycDocument
    {
        public string ClientId => PartitionKey;
        public string DocumentId => RowKey;
        public string Type { get; set; }
        public string Mime { get; set; }
        public string FileName { get; set; }
        public DateTime DateTime { get; set; }

        public static string GeneratePartitionKey(string clientId)
        {
            return clientId;
        }

        public static string GenerateRowKey(string documentId)
        {
            return documentId;
        }

        public static KycDocumentEntity Create(IKycDocument src)
        {
            return new KycDocumentEntity
            {
                PartitionKey = GeneratePartitionKey(src.ClientId),
                RowKey = GenerateRowKey(src.DocumentId ?? Guid.NewGuid().ToString()),
                Type = src.Type,
                Mime = src.Mime,
                DateTime = src.DateTime,
                FileName = src.FileName
            };
        }
    }

    public class KycDocumentsRepository : IKycDocumentsRepository
    {
        private readonly INoSQLTableStorage<KycDocumentEntity> _tableStorage;

        public KycDocumentsRepository(INoSQLTableStorage<KycDocumentEntity> tableStorage)
        {
            _tableStorage = tableStorage;
        }

        public async Task<IKycDocument> AddAsync(IKycDocument kycDocument)
        {
            var newDocument = KycDocumentEntity.Create(kycDocument);
            await _tableStorage.InsertAsync(newDocument);
            return newDocument;
        }

        public async Task<IEnumerable<IKycDocument>> GetAsync(string clientId)
        {
            var partitionKey = KycDocumentEntity.GeneratePartitionKey(clientId);
            return await _tableStorage.GetDataAsync(partitionKey);
        }

        public async Task<IEnumerable<IKycDocument>> GetOneEachTypeAsync(string clientId)
        {
            var partitionKey = KycDocumentEntity.GeneratePartitionKey(clientId);
            var docs = (await _tableStorage.GetDataAsync(partitionKey)).ToList();

            var result = new List<IKycDocument>();
            var latestIdCard =
                docs.OrderByDescending(x => x.DateTime).FirstOrDefault(x => x.Type == KycDocumentTypes.IdCard);
            if (latestIdCard != null)
                result.Add(latestIdCard);
            var latestSelfie =
                docs.OrderByDescending(x => x.DateTime).FirstOrDefault(x => x.Type == KycDocumentTypes.Selfie);
            if (latestSelfie != null)
                result.Add(latestSelfie);
            var latestProofOfAddress =
                docs.OrderByDescending(x => x.DateTime).FirstOrDefault(x => x.Type == KycDocumentTypes.ProofOfAddress);
            if (latestProofOfAddress != null)
                result.Add(latestProofOfAddress);

            return result;
        }

        public async Task<IKycDocument> DeleteAsync(string clientId, string documentId)
        {
            var partitionKey = KycDocumentEntity.GeneratePartitionKey(clientId);
            var rowKey = KycDocumentEntity.GenerateRowKey(documentId);
            return await _tableStorage.DeleteAsync(partitionKey, rowKey);
        }
    }
}