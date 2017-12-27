using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Assets.AssetGroup
{
    public interface IAssetGroup
    {
        string Name { get; set; }
        bool IsIosDevice { get; set; }
        bool ClientsCanCashInViaBankCards { get; set; }
    }

    public class AssetGroup : IAssetGroup
    {
        public string Name { get; set; }
        public bool IsIosDevice { get; set; }
        public bool ClientsCanCashInViaBankCards { get; set; }
    }

    public interface IAssetGroupRepository
    {
        Task RegisterGroup(string name, bool isIosDevice,  bool clientsCanCashInViaBankCards);
        Task EditGroup(IAssetGroup assetGroup);
        Task RemoveGroup(string name);
        Task<IAssetGroup> GetAsync(string name);
        Task<IEnumerable<IAssetGroup>> GetAllGroups();

        Task AddClientToGroup(string clientId, string group);
        Task RemoveClientFromGroup(string clientId, string group);
        Task<IEnumerable<string>> GetClientIdsForGroup(string group);

        Task AddAssetToGroup(string assetId, string group);
        Task RemoveAssetFromGroup(string assetId, string group);
        Task<IEnumerable<string>> GetAssetIdsForGroup(string group);

        Task<IEnumerable<string>> GetAssetIdsForClient(string clientId, bool isIosDevice);
        Task<bool> CanClientCashInViaBankCard(string clientId, bool isIosDevice);
    }
}
