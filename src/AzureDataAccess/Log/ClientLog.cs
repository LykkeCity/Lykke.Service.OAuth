using System;
using System.Threading.Tasks;
using AzureStorage;
using Core.Log;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureDataAccess.Log
{
    public class ClientLogItem : TableEntity
    {
        public string Data { get; set; }

        public static ClientLogItem Create(string userId, string data)
        {
            return new ClientLogItem
            {
                PartitionKey = userId,
                Data = data
            };
        }
    }

    public class ClientLog : IClientLog
    {
        private readonly INoSQLTableStorage<ClientLogItem> _tableStorage;

        public ClientLog(INoSQLTableStorage<ClientLogItem> tableStorage)
        {
            _tableStorage = tableStorage;
        }

        public Task WriteAsync(string userId, string dataId)
        {
            var newEntity = ClientLogItem.Create(userId, dataId);
            return _tableStorage.InsertAndGenerateRowKeyAsDateTimeAsync(newEntity, DateTime.UtcNow);
        }
    }
}