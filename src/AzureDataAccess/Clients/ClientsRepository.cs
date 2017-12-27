using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AzureStorage;
using AzureStorage.Tables.Templates.Index;
using Common.PasswordTools;
using Core.Clients;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureDataAccess.Clients
{
    public class ClientAccountEntity : TableEntity, IClientAccount, IPasswordKeeping
    {
        public static string GeneratePartitionKey()
        {
            return "Trader";
        }

        public static string GenerateRowKey(string id)
        {
            return id;
        }

        public DateTime Registered { get; set; }
        public string Id => RowKey;
        public string Email { get; set; }
        public string Phone { get; set; }
        public string NotificationsId { get; set; }
        public string Salt { get; set; }
        public string Hash { get; set; }

        public static ClientAccountEntity CreateNew(IClientAccount clientAccount, string password)
        {
            var result = new ClientAccountEntity
            {
                PartitionKey = GeneratePartitionKey(),
                RowKey = Guid.NewGuid().ToString(),
                NotificationsId = Guid.NewGuid().ToString("N"),
                Email = clientAccount.Email.ToLower(),
                Phone = clientAccount.Phone,
                Registered = clientAccount.Registered
            };

            result.SetPassword(password);

            return result;
        }
    }


    public class ClientsRepository : IClientAccountsRepository
    {
        private readonly INoSQLTableStorage<ClientAccountEntity> _clientsTablestorage;
        private readonly INoSQLTableStorage<AzureIndex> _emailIndices;

        private const string IndexEmail = "IndexEmail";

        public ClientsRepository(INoSQLTableStorage<ClientAccountEntity> clientsTablestorage, INoSQLTableStorage<AzureIndex> emailIndices)
        {
            _clientsTablestorage = clientsTablestorage;
            _emailIndices = emailIndices;
        }

        public async Task<IClientAccount> RegisterAsync(IClientAccount clientAccount, string password)
        {
            var newEntity = ClientAccountEntity.CreateNew(clientAccount, password);
            var indexEntity = AzureIndex.Create(IndexEmail, newEntity.Email, newEntity);

            await _emailIndices.InsertAsync(indexEntity);
            await _clientsTablestorage.InsertAsync(newEntity);

            return newEntity;
        }

        public Task ChangePhoneAsync(string clientId, string phoneNumber)
        {
            var partitionKey = ClientAccountEntity.GeneratePartitionKey();
            var rowKey = ClientAccountEntity.GenerateRowKey(clientId);

            return _clientsTablestorage.ReplaceAsync(partitionKey, rowKey, itm =>
            {
                itm.Phone = phoneNumber;
                return itm;
            });
        }

        public async Task<bool> IsTraderWithEmailExistsAsync(string email)
        {
            if (string.IsNullOrEmpty(email))
                return false;

            var indexEntity = await _emailIndices.GetDataAsync(IndexEmail, email.ToLower());

            return indexEntity != null;
        }

        public async Task<IClientAccount> AuthenticateAsync(string email, string password)
        {
            if (email == null || password == null)
                return null;

            var indexEntity = await _emailIndices.GetDataAsync(IndexEmail, email.ToLower());

            if (indexEntity == null)
                return null;

            var entity = await _clientsTablestorage.GetDataAsync(indexEntity);

            if (entity == null)
                return null;


            return entity.CheckPassword(password) ? entity : null;

        }

        public Task ChangePassword(string clientId, string newPassword)
        {
            var partitionKey = ClientAccountEntity.GeneratePartitionKey();
            var rowKey = ClientAccountEntity.GenerateRowKey(clientId);

            return _clientsTablestorage.ReplaceAsync(partitionKey, rowKey, itm =>
            {
                itm.SetPassword(newPassword);
                return itm;
            });
        }

        public async Task<IClientAccount> GetByIdAsync(string id)
        {
            var partitionKey = ClientAccountEntity.GeneratePartitionKey();
            var rowKey = ClientAccountEntity.GenerateRowKey(id);

            return await _clientsTablestorage.GetDataAsync(partitionKey, rowKey);
        }

        public async Task<IEnumerable<IClientAccount>> GetByIdAsync(string[] ids)
        {
            var partitionKey = ClientAccountEntity.GeneratePartitionKey();
            return await _clientsTablestorage.GetDataAsync(partitionKey, ids);
        }

        public async Task<IClientAccount> GetByEmailAsync(string email)
        {
            if (string.IsNullOrEmpty(email))
                return null;

            return await _clientsTablestorage.GetDataAsync(_emailIndices, IndexEmail, email.ToLower());
        }


        public async Task<string> GenerateNotificationsId(string clientId)
        {
            var partitionKey = ClientAccountEntity.GeneratePartitionKey();
            var rowKey = ClientAccountEntity.GenerateRowKey(clientId);

            var updated = await _clientsTablestorage.ReplaceAsync(partitionKey, rowKey, itm =>
            {
                itm.NotificationsId = Guid.NewGuid().ToString("N");
                return itm;
            });

            return updated.NotificationsId;
        }

        public async Task<bool> IsPasswordCorrect(string clientId, string password)
        {
            if (string.IsNullOrEmpty(clientId))
                return false;

            var entity = await _clientsTablestorage.GetDataAsync(ClientAccountEntity.GeneratePartitionKey(), ClientAccountEntity.GenerateRowKey(clientId));
            if (entity != null)
                return entity.CheckPassword(password);

            return false;
        }
    }
}
