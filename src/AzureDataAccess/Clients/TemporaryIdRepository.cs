using System;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage;
using Core.Clients;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureDataAccess.Clients
{
    public class TemporaryIdRecord : TableEntity
    {
        public string TemporaryId => PartitionKey;
        public string RealId => RowKey;

        public static TemporaryIdRecord Create(string realId, string temporaryId)
        {
            return new TemporaryIdRecord
            {
                RowKey = realId,
                PartitionKey = temporaryId
            };
        }
    }

    public class TemporaryIdRepository : ITemporaryIdRepository
    {
        private readonly INoSQLTableStorage<TemporaryIdRecord> _tableStorage;

        public TemporaryIdRepository(INoSQLTableStorage<TemporaryIdRecord> tableStorage)
        {
            _tableStorage = tableStorage;
        }

        public async Task<string> GenerateTemporaryId(string realId)
        {
            var entity = TemporaryIdRecord.Create(realId, Guid.NewGuid().ToString("N"));
            await _tableStorage.InsertAsync(entity);
            return entity.TemporaryId;
        }

        public async Task<string> GetRealId(string temporaryId)
        {
            var entity = (await _tableStorage.GetDataAsync(temporaryId)).FirstOrDefault();
            return entity?.RealId;
        }

        public async Task RemoveTemporaryIdRecord(string temporaryId)
        {
            var entity = (await _tableStorage.GetDataAsync(temporaryId)).First();
            await _tableStorage.DeleteAsync(entity);
        }
    }
}