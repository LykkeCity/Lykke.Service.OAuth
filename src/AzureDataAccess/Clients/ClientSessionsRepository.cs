using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AzureStorage;
using Core.Clients;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureDataAccess.Clients
{
    public class ClientSessionEntity : TableEntity, IClientSession
    {
        public static class ByToken
        {
            public static string GeneratePartitionKey()
            {
                return "Sess";
            }

            public static string GenerateRowKey(string token)
            {
                return token;
            }
        }

        public static class ByClient
        {
            public static string GeneratePartitionKey(string clientId)
            {
                return clientId;
            }

            public static string GenerateRowKey(string token)
            {
                return token;
            }
        }

        public string ClientId { get; set; }
        public string Token => RowKey;
        public string ClientInfo { get; set; }
        public DateTime Registered { get; set; }
        public DateTime LastAction { get; set; }

        internal void UpdateClientInfo(string clientInfo)
        {
            ClientInfo = clientInfo;
            LastAction = DateTime.UtcNow;
        }

        public static ClientSessionEntity CreateByToken(string clientId, string token, string clientInfo)
        {
            return new ClientSessionEntity
            {
                PartitionKey = ByToken.GeneratePartitionKey(),
                RowKey = ByToken.GenerateRowKey(token),
                ClientId = clientId,
                Registered = DateTime.UtcNow,
                LastAction = DateTime.UtcNow,
                ClientInfo = clientInfo
            };
        }

        public static ClientSessionEntity CreateByClient(string clientId, string token, string clientInfo)
        {
            return new ClientSessionEntity
            {
                PartitionKey = ByClient.GeneratePartitionKey(clientId),
                RowKey = ByClient.GenerateRowKey(token),
                ClientId = clientId,
                Registered = DateTime.UtcNow,
                LastAction = DateTime.UtcNow,
                ClientInfo = clientInfo
            };
        }

    }


    public class ClientSessionsRepository : IClientsSessionsRepository
    {
        private readonly INoSQLTableStorage<ClientSessionEntity> _tableStorage;

        public ClientSessionsRepository(INoSQLTableStorage<ClientSessionEntity> tableStorage)
        {
            _tableStorage = tableStorage;
        }

        public Task SaveAsync(string clientId, string sessionId, string clientInfo)
        {
            return Task.WhenAll(
                _tableStorage.InsertAsync(ClientSessionEntity.CreateByToken(clientId, sessionId, clientInfo)),
                _tableStorage.InsertAsync(ClientSessionEntity.CreateByClient(clientId, sessionId, clientInfo))
                );
        }

        public async Task<IClientSession> GetAsync(string sessionId)
        {
            var partitionKey = ClientSessionEntity.ByToken.GeneratePartitionKey();
            var rowKey = ClientSessionEntity.ByToken.GenerateRowKey(sessionId);
            return await _tableStorage.GetDataAsync(partitionKey, rowKey);
        }

        public async Task<IEnumerable<IClientSession>> GetByClientAsync(string clientId)
        {
            var partitionKey = ClientSessionEntity.ByClient.GeneratePartitionKey(clientId);
            return await _tableStorage.GetDataAsync(partitionKey);
        }


        public async Task UpdateClientInfoAsync(string clientId, string token, string clientInfo)
        {

            var partitionKey = ClientSessionEntity.ByClient.GeneratePartitionKey(clientId);
            var rowKey = ClientSessionEntity.ByClient.GenerateRowKey(token);

            await _tableStorage.ReplaceAsync(partitionKey, rowKey, entity =>
            {
                entity.UpdateClientInfo(clientInfo);
                return entity;
            });

            partitionKey = ClientSessionEntity.ByToken.GeneratePartitionKey();
            rowKey = ClientSessionEntity.ByToken.GenerateRowKey(token);

            await _tableStorage.ReplaceAsync(partitionKey, rowKey, entity =>
            {
                entity.UpdateClientInfo(clientInfo);
                return entity;
            });

        }

        public async Task DeleteSessionAsync(string clientId, string token)
        {

            var partitionKey = ClientSessionEntity.ByToken.GeneratePartitionKey();
            var rowKey = ClientSessionEntity.ByToken.GenerateRowKey(token);
            await _tableStorage.DeleteAsync(partitionKey, rowKey);

            partitionKey = ClientSessionEntity.ByClient.GeneratePartitionKey(clientId);
            rowKey = ClientSessionEntity.ByClient.GenerateRowKey(token);
            await _tableStorage.DeleteAsync(partitionKey, rowKey);

        }
    }
}
