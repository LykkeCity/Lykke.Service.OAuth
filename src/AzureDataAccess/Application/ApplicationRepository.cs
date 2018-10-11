using System.Threading.Tasks;
using AzureStorage;
using Common;
using Core.Application;

namespace AzureDataAccess.Application
{
    public class ApplicationRepository : IApplicationRepository
    {
        private readonly INoSQLTableStorage<ApplicationEntity> _applicationTablestorage;

        public ApplicationRepository(INoSQLTableStorage<ApplicationEntity> applicationTablestorage)
        {
            _applicationTablestorage = applicationTablestorage;
        }

        public async Task<Core.Application.ClientApplication> GetByIdAsync(string id)
        {
            if (!id.IsValidPartitionOrRowKey())
            {
                return null;
            }
            var partitionKey = ApplicationEntity.GeneratePartitionKey();
            var rowKey = ApplicationEntity.GenerateRowKey(id);

            var application = await _applicationTablestorage.GetDataAsync(partitionKey, rowKey);
            return Core.Application.ClientApplication.Create(application);
        }
    }
}
