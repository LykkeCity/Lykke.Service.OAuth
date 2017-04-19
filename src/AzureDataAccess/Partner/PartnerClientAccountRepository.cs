using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AzureStorage;
using Core.Partner;
using Microsoft.WindowsAzure.Storage.Table;
using Common.PasswordTools;

namespace AzureDataAccess.Partner
{
    public class PartnerClientAccountEntity : TableEntity, IPartnerClientAccount, IPasswordKeeping
    {
        public string PublicId { get; set; }

        public string ClientId { get; set; }

        public DateTime Created { get; set; }

        public string Salt { get; set; }

        public string Hash { get; set; }

        public static string GeneratePartition(string publicId)
        {
            return $"PartnerClientAccount_{publicId}";
        }

        public static string GenerateRowKey(string clientId)
        {
            return clientId;
        }

        public static PartnerClientAccountEntity Create(IPartnerClientAccount pClientAccount, string password)
        {
            PartnerClientAccountEntity result = new PartnerClientAccountEntity
            {
                PartitionKey = GeneratePartition(pClientAccount.PublicId),
                RowKey = GenerateRowKey(pClientAccount.ClientId),
                ClientId = pClientAccount.ClientId,
                PublicId = pClientAccount.PublicId,
                Created = pClientAccount.Created
            };

            result.SetPassword(password);
            return result;
        }
    }

    public class PartnerClientAccountRepository : IPartnerClientAccountRepository
    {
        private readonly INoSQLTableStorage<PartnerClientAccountEntity> _tableStorage;
        public PartnerClientAccountRepository(INoSQLTableStorage<PartnerClientAccountEntity> tableStorage)
        {
            _tableStorage = tableStorage;
        }

        public async Task<IPartnerClientAccount> AuthenticateAsync(string clientId, string publicId, string password)
        {
            if (string.IsNullOrEmpty(clientId)
                || string.IsNullOrEmpty(publicId)
                || string.IsNullOrEmpty(password))
            {
                return null;
            }

            PartnerClientAccountEntity partnerClientAccount = await
                _tableStorage.GetDataAsync(PartnerClientAccountEntity.GeneratePartition(publicId),
                PartnerClientAccountEntity.GenerateRowKey(clientId));
            IPartnerClientAccount result = partnerClientAccount != null &&
                partnerClientAccount.CheckPassword(password) ? partnerClientAccount : null;

            return result;
        }

        public Task ChangePassword(string clientId, string publicId, string newPassword)
        {
            string partitionKey = PartnerClientAccountEntity.GeneratePartition(publicId);
            string rowKey = PartnerClientAccountEntity.GenerateRowKey(clientId);

            return _tableStorage.ReplaceAsync(partitionKey, rowKey, itm =>
            {
                itm.SetPassword(newPassword);
                return itm;
            });
        }

        public Task<IEnumerable<IPartnerClientAccount>> GetForClientAsync(string clientId)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<IPartnerClientAccount>> GetForPartnerAsync(string publicId)
        {
            IEnumerable<IPartnerClientAccount> result = await _tableStorage.GetDataAsync(PartnerClientAccountEntity.GeneratePartition(publicId));

            return result;
        }

        public async Task<bool> IsTraderRegisteredForPartnerAsync(string clientId, string publicId)
        {
            string partitionKey = PartnerClientAccountEntity.GeneratePartition(publicId);
            string rowKey = PartnerClientAccountEntity.GenerateRowKey(clientId);

            PartnerClientAccountEntity result = await _tableStorage.GetDataAsync(partitionKey, rowKey);

            return result != null;
        }

        public async Task RegisterAsync(IPartnerClientAccount partnerClient, string password)
        {
            PartnerClientAccountEntity pclientAccount = PartnerClientAccountEntity.Create(partnerClient, password);
            await _tableStorage.InsertAsync(pclientAccount);
        }
    }
}
