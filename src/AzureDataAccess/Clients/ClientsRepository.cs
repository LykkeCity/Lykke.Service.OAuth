using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AzureStorage;
using AzureStorage.Tables.Templates.Index;
using Core.Clients;
using Microsoft.WindowsAzure.Storage.Table;
using Common.PasswordTools;
using System.Linq;

namespace AzureDataAccess.Clients
{
    public class ClientPartnerRelationEntity : TableEntity
    {
        public static string GeneratePartitionKey(string email)
        {
            return $"TraderPartnerRelation_{email}";
        }

        public static string GenerateRowKey(string partnerId)
        {
            return partnerId;
        }

        public DateTime Registered { get; set; }
        public string Id => RowKey;
        public string Email { get; set; }
        public string PartnerId { get; set; }
        public string ClientId { get; set; }

        public static ClientPartnerRelationEntity CreateNew(string email, string clientId, string partnerId)
        {
            string partnerPublicId = partnerId ?? "";
            string clientEmail = email.ToLower();
            var result = new ClientPartnerRelationEntity
            {
                PartitionKey = GeneratePartitionKey(clientEmail),
                RowKey = GenerateRowKey(partnerPublicId),
                Email = clientEmail,
                PartnerId = partnerPublicId,
                ClientId = clientId,
                Registered = DateTime.UtcNow
            };

            return result;
        }
    }

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
        public string Pin { get; set; }
        public string NotificationsId { get; set; }
        public string Salt { get; set; }
        public string Hash { get; set; }
        public string PartnerId { get; set; }

        public static ClientAccountEntity CreateNew(IClientAccount clientAccount, string password)
        {
            var result = new ClientAccountEntity
            {
                PartitionKey = GeneratePartitionKey(),
                RowKey = Guid.NewGuid().ToString(),
                NotificationsId = Guid.NewGuid().ToString("N"),
                Email = clientAccount.Email.ToLower(),
                Phone = clientAccount.Phone,
                Registered = clientAccount.Registered,
                PartnerId = clientAccount.PartnerId
            };

            result.SetPassword(password);

            return result;
        }
    }


    public class ClientsRepository : IClientAccountsRepository
    {
        private readonly INoSQLTableStorage<ClientAccountEntity> _clientsTablestorage;
        private readonly INoSQLTableStorage<AzureIndex> _emailIndices;
        private readonly INoSQLTableStorage<ClientPartnerRelationEntity> _clientPartnerTablestorage;
        private const string IndexEmail = "IndexEmail";

        public ClientsRepository(INoSQLTableStorage<ClientAccountEntity> clientsTablestorage,
            INoSQLTableStorage<ClientPartnerRelationEntity> clientPartnerTablestorage,
            INoSQLTableStorage<AzureIndex> emailIndices)
        {
            _clientsTablestorage = clientsTablestorage;
            _emailIndices = emailIndices;
            _clientPartnerTablestorage = clientPartnerTablestorage;
        }

        public async Task<IClientAccount> RegisterAsync(IClientAccount clientAccount, string password)
        {
            var newEntity = ClientAccountEntity.CreateNew(clientAccount, password);
            string partnerId = clientAccount.PartnerId;
            string indexRowKey = GetEmailPartnerIndexRowKey(newEntity);
            var indexEntity = AzureIndex.Create(IndexEmail, indexRowKey, newEntity);
            ClientPartnerRelationEntity clientPartner =
                ClientPartnerRelationEntity.CreateNew(clientAccount.Email, newEntity.Id, newEntity.PartnerId);

            await _emailIndices.InsertAsync(indexEntity);
            await _clientsTablestorage.InsertAsync(newEntity);
            await _clientPartnerTablestorage.InsertAsync(clientPartner);

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

        public async Task<bool> IsTraderWithEmailExistsAsync(string email, string partnerId = null)
        {
            if (string.IsNullOrEmpty(email))
                return false;

            string indexRowKey = GetEmailPartnerIndexRowKey(email, partnerId);
            var indexEntity = await _emailIndices.GetDataAsync(IndexEmail, indexRowKey);

            return indexEntity != null;
        }

        public async Task<IClientAccount> AuthenticateAsync(string email, string password, string partnerId = null)
        {
            if (email == null || password == null)
                return null;

            string indexRowKey = GetEmailPartnerIndexRowKey(email, partnerId);
            var indexEntity = await _emailIndices.GetDataAsync(IndexEmail, indexRowKey);

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

        public async Task<IClientAccount> GetByEmailAndPartnerIdAsync(string email, string partnerId)
        {
            if (string.IsNullOrEmpty(email))
                return null;

            return await _clientsTablestorage.GetDataAsync(_emailIndices, IndexEmail, GetEmailPartnerIndexRowKey(email, partnerId));
        }
        public async Task<IEnumerable<IClientAccount>> GetByEmailAsync(string email)
        {
            if (string.IsNullOrEmpty(email))
                return null;

            IEnumerable<ClientPartnerRelationEntity> relations =
                await _clientPartnerTablestorage.GetDataAsync(ClientPartnerRelationEntity.GeneratePartitionKey(email));
            IEnumerable<string> rowKeys = relations.Select(x => x.ClientId);

            return await _clientsTablestorage.GetDataAsync(ClientAccountEntity.GeneratePartitionKey(), rowKeys);
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

        public Task SetPin(string clientId, string newPin)
        {
            var partitionKey = ClientAccountEntity.GeneratePartitionKey();
            var rowKey = ClientAccountEntity.GenerateRowKey(clientId);

            return _clientsTablestorage.ReplaceAsync(partitionKey, rowKey, itm =>
            {
                itm.Pin = newPin;
                return itm;
            });
        }

        private string GetEmailPartnerIndexRowKey(ClientAccountEntity clientAccount)
        {
            return GetEmailPartnerIndexRowKey(clientAccount.Email, clientAccount.PartnerId);
        }

        private string GetEmailPartnerIndexRowKey(string email, string partnerId)
        {
            string lowEmail = email.ToLower();
            return string.IsNullOrEmpty(partnerId) ? $"{lowEmail}" : $"{lowEmail}_{partnerId}";
        }
    }
}
