using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AzureStorage;
using Core.Partner;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureDataAccess.Partner
{
    public class PartnerAccountPolicyEntity : TableEntity, IPartnerAccountPolicy
    {
        public string PublicId { get; set; }

        public bool UseDifferentCredentials { get; set; }

        public bool UseDifferentWallets { get; set; }

        public static string GeneratePartition()
        {
            return "PartnerAccountPolicy";
        }

        public static string GenerateRowKey(string id)
        {
            return id;
        }

        public static PartnerAccountPolicyEntity Create(IPartnerAccountPolicy policy)
        {
            return new PartnerAccountPolicyEntity
            {
                PartitionKey = GeneratePartition(),
                RowKey = GenerateRowKey(policy.PublicId),
                UseDifferentWallets = policy.UseDifferentWallets,
                UseDifferentCredentials = policy.UseDifferentCredentials,
                PublicId = policy.PublicId
            };
        }

        public static PartnerAccountPolicyEntity Update(IPartnerAccountPolicy from, IPartnerAccountPolicy to)
        {
            from.UseDifferentWallets = to.UseDifferentWallets;
            from.UseDifferentCredentials = to.UseDifferentCredentials;
            from.PublicId = to.PublicId;

            return Create(from);
        }
    }

    public class PartnerAccountPolicyRepository : IPartnerAccountPolicyRepository
    {
        private readonly INoSQLTableStorage<PartnerAccountPolicyEntity> _tableStorage;

        public PartnerAccountPolicyRepository(INoSQLTableStorage<PartnerAccountPolicyEntity> tableStorage)
        {
            _tableStorage = tableStorage;
        }

        public Task CreateAsync(IPartnerAccountPolicy partnerAccountPolicy)
        {
            var entity = PartnerAccountPolicyEntity.Create(partnerAccountPolicy);
            return _tableStorage.InsertAsync(entity);
        }

        public async Task CreateOrUpdateAsync(IPartnerAccountPolicy partnerAccountPolicy)
        {
            var entity = PartnerAccountPolicyEntity.Create(partnerAccountPolicy);
            var oldEntity = await GetAsync(entity.PublicId);
            if (oldEntity == null)
            {
                await CreateAsync(entity);
            }
            else
            {
                var entityNew = PartnerAccountPolicyEntity.Update(oldEntity, partnerAccountPolicy);
                await _tableStorage.InsertOrReplaceAsync(entityNew);
            }
        }

        public async Task UpdateAsync(IPartnerAccountPolicy partnerAccountPolicy)
        {
            var entity =
                    await
                        _tableStorage.GetDataAsync(PartnerAccountPolicyEntity.GeneratePartition(),
                            PartnerAccountPolicyEntity.GenerateRowKey(partnerAccountPolicy.PublicId));

            PartnerAccountPolicyEntity.Update(entity, partnerAccountPolicy);

            await _tableStorage.InsertOrReplaceAsync(entity);
        }

        public async Task<IEnumerable<IPartnerAccountPolicy>> GetPoliciesAsync()
        {
            return await _tableStorage.GetDataAsync(PartnerAccountPolicyEntity.GeneratePartition());
        }

        public async Task<IPartnerAccountPolicy> GetAsync(string publicId)
        {
            return await _tableStorage.GetDataAsync(PartnerAccountPolicyEntity.GeneratePartition(),
                PartnerAccountPolicyEntity.GenerateRowKey(publicId));
        }

        public Task RemoveAsync(string publicId)
        {
            return _tableStorage.DeleteAsync(PartnerAccountPolicyEntity.GeneratePartition(), 
                PartnerAccountPolicyEntity.GenerateRowKey(publicId));
        }
    }
}
