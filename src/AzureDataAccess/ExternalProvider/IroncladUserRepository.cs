using System.Threading.Tasks;
using AzureStorage;
using Core.ExternalProvider;

namespace AzureDataAccess.ExternalProvider
{
    public class IroncladUserRepository : IIroncladUserRepository
    {
        private readonly INoSQLTableStorage<IroncladUserEntity> _storage;

        public IroncladUserRepository(INoSQLTableStorage<IroncladUserEntity> storage)
        {
            _storage = storage;
        }

        public Task<bool> AddAsync(IroncladUserBinding ironcladUserBinding)
        {
            var entity = IroncladUserEntity.FromDomain(ironcladUserBinding);

            return _storage.CreateIfNotExistsAsync(entity);
        }

        public async Task<IroncladUserBinding> GetByIdAsync(string ironcladUserId)
        {
            var partitionKey = IroncladUserEntity.GeneratePartitionKey(ironcladUserId);
            var rowKey = IroncladUserEntity.GenerateRowKey(ironcladUserId);
            var entity = await _storage.GetDataAsync(partitionKey, rowKey);
            return IroncladUserEntity.ToDomain(entity);
        }
    }
}
