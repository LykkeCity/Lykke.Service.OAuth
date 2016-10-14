using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage;
using Core.Assets.AssetGroup;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureRepositories.Assets
{
    public class AssetGroupEntity : TableEntity, IAssetGroup
    {
        public string Name { get; set; }
        public bool IsIosDevice { get; set; }
        public string AssetId { get; set; }
        public string ClientId { get; set; }

        public bool ClientsCanCashInViaBankCards { get; set; }

        public static class Record
        {
            public static string GenerateRowKey(string group)
            {
                return group;
            }

            public static string GeneratePartitionKey()
            {
                return "AssetGroup";
            }

            public static AssetGroupEntity Create(string group, bool isIosDevice, bool clientsCanCashInViaBankCards)
            {
                return new AssetGroupEntity
                {
                    RowKey = GenerateRowKey(group),
                    PartitionKey = GeneratePartitionKey(),
                    Name = group,
                    IsIosDevice = isIosDevice,
                    ClientsCanCashInViaBankCards = clientsCanCashInViaBankCards
                };
            }

            public static AssetGroupEntity Create(IAssetGroup assetGroup)
            {
                return new AssetGroupEntity
                {
                    RowKey = GenerateRowKey(assetGroup.Name),
                    PartitionKey = GeneratePartitionKey(),
                    Name = assetGroup.Name,
                    IsIosDevice = assetGroup.IsIosDevice,
                    ClientsCanCashInViaBankCards = assetGroup.ClientsCanCashInViaBankCards
                };
            }
        }

        public static class ClientGroupLink
        {
            public static string GenerateRowKey(string clientId)
            {
                return clientId;
            }

            public static string GeneratePartitionKey(string group)
            {
                return $"ClientGroupLink_{group}";
            }

            public static AssetGroupEntity Create(string group, string clientId, bool isIosDevice, bool clientsCanCashInViaBankCards)
            {
                return new AssetGroupEntity
                {
                    RowKey = GenerateRowKey(clientId),
                    PartitionKey = GeneratePartitionKey(group),
                    Name = group,
                    ClientId = clientId,
                    IsIosDevice = isIosDevice,
                    ClientsCanCashInViaBankCards = clientsCanCashInViaBankCards
                };
            }

            public static void Update(AssetGroupEntity entity, IAssetGroup assetGroup)
            {
                entity.Name = assetGroup.Name;
                entity.ClientsCanCashInViaBankCards = assetGroup.ClientsCanCashInViaBankCards;
                entity.IsIosDevice = assetGroup.IsIosDevice;
            }
        }

        public static class GroupClientLink
        {
            public static string GenerateRowKey(string group)
            {
                return group;
            }

            public static string GeneratePartitionKey(string clientId)
            {
                return $"GroupClientLink_{clientId}";
            }

            public static AssetGroupEntity Create(string group, string clientId, bool isIosDevice, bool clientsCanCashInViaBankCards)
            {
                return new AssetGroupEntity
                {
                    RowKey = GenerateRowKey(group),
                    PartitionKey = GeneratePartitionKey(clientId),
                    Name = group,
                    ClientId = clientId,
                    IsIosDevice = isIosDevice,
                    ClientsCanCashInViaBankCards = clientsCanCashInViaBankCards
                    
                };
            }

            public static void Update(AssetGroupEntity entity, IAssetGroup assetGroup)
            {
                entity.Name = assetGroup.Name;
                entity.ClientsCanCashInViaBankCards = assetGroup.ClientsCanCashInViaBankCards;
                entity.IsIosDevice = assetGroup.IsIosDevice;
            }
        }

        public static class AssetLink
        {
            public static string GenerateRowKey(string assetId)
            {
                return assetId;
            }

            public static string GeneratePartitionKey(string group)
            {
                return $"AssetLink_{group}";
            }

            public static AssetGroupEntity Create(string group, string assetId, bool isIosDevice, bool clientsCanCashInViaBankCards)
            {
                return new AssetGroupEntity
                {
                    RowKey = GenerateRowKey(assetId),
                    PartitionKey = GeneratePartitionKey(group),
                    Name = group,
                    AssetId = assetId,
                    IsIosDevice = isIosDevice,
                    ClientsCanCashInViaBankCards = clientsCanCashInViaBankCards
                };
            }
        }
    }

    public class AssetGroupRepository : IAssetGroupRepository
    {
        readonly INoSQLTableStorage<AssetGroupEntity> _tableStorage;

        public AssetGroupRepository(INoSQLTableStorage<AssetGroupEntity> tableStorage)
        {
            _tableStorage = tableStorage;
        }

        public Task RegisterGroup(string group, bool isIosDevice,  bool clientsCanCashInViaBankCards)
        {
            var entity = AssetGroupEntity.Record.Create(group, isIosDevice, clientsCanCashInViaBankCards);
            return _tableStorage.InsertOrReplaceAsync(entity);
        }

        public async Task EditGroup(IAssetGroup assetGroup)
        {
            var entity = AssetGroupEntity.Record.Create(assetGroup);
            await _tableStorage.InsertOrMergeAsync(entity);

            var updatedGroup = await _tableStorage.GetDataAsync(AssetGroupEntity.Record.GeneratePartitionKey(),
                AssetGroupEntity.Record.GenerateRowKey(assetGroup.Name));

            var clients = (await GetClientIdsForGroup(assetGroup.Name)).ToArray();

            if (clients.Any())
            {
                foreach (var clientId in clients)
                {
                    var clientGroupLink = await _tableStorage
                        .GetDataAsync(AssetGroupEntity.ClientGroupLink.GeneratePartitionKey(updatedGroup.Name),
                            AssetGroupEntity.ClientGroupLink.GenerateRowKey(clientId));
                    AssetGroupEntity.ClientGroupLink.Update(clientGroupLink, updatedGroup);

                    var groupClientLink = await _tableStorage
                        .GetDataAsync(AssetGroupEntity.GroupClientLink.GeneratePartitionKey(clientId),
                            AssetGroupEntity.GroupClientLink.GenerateRowKey(updatedGroup.Name));
                    AssetGroupEntity.GroupClientLink.Update(groupClientLink, updatedGroup);

                    await _tableStorage.InsertOrMergeAsync(clientGroupLink);
                    await _tableStorage.InsertOrMergeAsync(groupClientLink);
                }
            }
        }

        public Task RemoveGroup(string group)
        {
            return _tableStorage.DeleteAsync(AssetGroupEntity.Record.GeneratePartitionKey(),
                AssetGroupEntity.Record.GenerateRowKey(group));
        }

        public async Task<IAssetGroup> GetAsync(string name)
        {
            return await _tableStorage.GetDataAsync(AssetGroupEntity.Record.GeneratePartitionKey(),
                AssetGroupEntity.Record.GenerateRowKey(name));
        }

        public async Task<IEnumerable<IAssetGroup>> GetAllGroups()
        {
            return await _tableStorage.GetDataAsync(AssetGroupEntity.Record.GeneratePartitionKey());
        }

        public async Task AddClientToGroup(string clientId, string group)
        {
            var groupRecord = await _tableStorage.GetDataAsync(AssetGroupEntity.Record.GeneratePartitionKey(),
                AssetGroupEntity.Record.GenerateRowKey(group));
            var cgEntity = AssetGroupEntity.ClientGroupLink.Create(group, clientId, groupRecord.IsIosDevice, groupRecord.ClientsCanCashInViaBankCards);
            var gcEntity = AssetGroupEntity.GroupClientLink.Create(group, clientId, groupRecord.IsIosDevice, groupRecord.ClientsCanCashInViaBankCards);
            await _tableStorage.InsertOrReplaceAsync(cgEntity);
            await _tableStorage.InsertOrReplaceAsync(gcEntity);
        }

        public async Task RemoveClientFromGroup(string clientId, string group)
        {
            await _tableStorage.DeleteAsync(AssetGroupEntity.ClientGroupLink.GeneratePartitionKey(group),
                AssetGroupEntity.ClientGroupLink.GenerateRowKey(clientId));
            await _tableStorage.DeleteAsync(AssetGroupEntity.GroupClientLink.GeneratePartitionKey(clientId),
                AssetGroupEntity.GroupClientLink.GenerateRowKey(group));
        }

        public async Task<IEnumerable<string>> GetClientIdsForGroup(string group)
        {
            return (await _tableStorage.GetDataAsync(AssetGroupEntity.ClientGroupLink.GeneratePartitionKey(group))).Select(x => x.RowKey);
        }

        public async Task AddAssetToGroup(string assetId, string group)
        {
            var groupRecord = await _tableStorage.GetDataAsync(AssetGroupEntity.Record.GeneratePartitionKey(),
                AssetGroupEntity.Record.GenerateRowKey(group));
            var entity = AssetGroupEntity.AssetLink.Create(group, assetId, groupRecord.IsIosDevice, groupRecord.ClientsCanCashInViaBankCards);
            await _tableStorage.InsertOrReplaceAsync(entity);
        }

        public Task RemoveAssetFromGroup(string assetId, string group)
        {
            return _tableStorage.DeleteAsync(AssetGroupEntity.AssetLink.GeneratePartitionKey(group),
                AssetGroupEntity.AssetLink.GenerateRowKey(assetId));
        }

        public async Task<IEnumerable<string>> GetAssetIdsForGroup(string group)
        {
            return (await _tableStorage.GetDataAsync(AssetGroupEntity.AssetLink.GeneratePartitionKey(group))).Select(x => x.RowKey);
        }

        public async Task<IEnumerable<string>> GetAssetIdsForClient(string clientId, bool isIosDevice)
        {
            var groups =
                (await _tableStorage.GetDataAsync(AssetGroupEntity.GroupClientLink.GeneratePartitionKey(clientId)))
                    .Where(x => x.IsIosDevice == isIosDevice).ToArray();

            if (groups.Any())
            {
                var result = new List<string>();

                foreach (var group in groups)
                {
                    result.AddRange((await _tableStorage.GetDataAsync(AssetGroupEntity.AssetLink.GeneratePartitionKey(group.Name)))
                        .Select(x => x.AssetId));
                }

                return result;
            }

            return null;
        }

        public async Task<bool> CanClientCashInViaBankCard(string clientId, bool isIosDevice)
        {
            var  assetsGroupsForClient =  await _tableStorage.GetDataAsync(AssetGroupEntity.GroupClientLink.GeneratePartitionKey(clientId));
            var clientIsNotAssignedToAnyGroup = !assetsGroupsForClient.Any();

            return clientIsNotAssignedToAnyGroup || assetsGroupsForClient.Any(p => p.ClientsCanCashInViaBankCards && p.IsIosDevice == isIosDevice);
        }
    }
}
