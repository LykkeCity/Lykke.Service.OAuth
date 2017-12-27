using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage;
using Common;
using Core.Kyc;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureDataAccess.Kyc
{
    public class KycEntity : TableEntity
    {
        internal const KycStatus DefaultStatus = KycStatus.NeedToFillData;

        public static string GeneratePartitionKey(KycStatus kycStatus)
        {
            return kycStatus.ToString();
        }

        public static string GenerateRowKey(string clientId)
        {
            return clientId;
        }

        internal string GetClientId()
        {
            return RowKey;
        }

        internal KycStatus GetSatus()
        {
            try
            {
                return PartitionKey.ParseEnum<KycStatus>();
            }
            catch (Exception)
            {
                return DefaultStatus;
            }
        }

        public static KycEntity Create(string clientId, KycStatus status)
        {
            return new KycEntity
            {
                PartitionKey = GeneratePartitionKey(status),
                RowKey = GenerateRowKey(clientId)
            };
        }
    }

    public class KycRepository : IKycRepository
    {
        private readonly INoSQLTableStorage<KycEntity> _tableStorage;

        public KycRepository(INoSQLTableStorage<KycEntity> tableStorage)
        {
            _tableStorage = tableStorage;
        }

        public async Task<KycStatus> GetKycStatusAsync(string clientId)
        {
            var rowKey = KycEntity.GenerateRowKey(clientId);
            var entity = (await _tableStorage.GetDataRowKeyOnlyAsync(rowKey)).FirstOrDefault();

            return entity?.GetSatus() ?? KycEntity.DefaultStatus;
        }

        public async Task<IEnumerable<string>> GetClientsByStatus(KycStatus kycStatus)
        {
            var partitionKey = KycEntity.GeneratePartitionKey(kycStatus);
            return (await _tableStorage.GetDataAsync(partitionKey)).Select(itm => itm.GetClientId());
        }

        public async Task SetStatusAsync(string clientId, KycStatus status)
        {
            var rowKey = KycEntity.GenerateRowKey(clientId);
            var entity = (await _tableStorage.GetDataRowKeyOnlyAsync(rowKey)).FirstOrDefault();

            if (entity != null)
                await _tableStorage.DeleteAsync(entity);

            if (status == KycEntity.DefaultStatus)
                return;

            entity = KycEntity.Create(clientId, status);
            await _tableStorage.InsertOrReplaceAsync(entity);
        }
    }
}