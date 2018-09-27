using System.Threading.Tasks;
using AzureStorage;
using Common;
using Core.Application;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureDataAccess.Application
{
    public class ApplicationEntity : TableEntity, IApplication
    {
        public string ApplicationId => RowKey;
        public string DisplayName { get; set; }
        public string RedirectUri { get; set; }
        public string Secret { get; set; }
        public string Type { get; set; }

        public static string GeneratePartitionKey()
        {
            return "InternalApplication";
        }

        public static string GenerateRowKey(string id)
        {
            return id;
        }
    }

    public class ApplicationRepository : IApplicationRepository
    {
        private readonly INoSQLTableStorage<ApplicationEntity> _applicationTablestorage;

        public ApplicationRepository(INoSQLTableStorage<ApplicationEntity> applicationTablestorage)
        {
            _applicationTablestorage = applicationTablestorage;
        }

        public async Task<Core.Application.Application> GetByIdAsync(string id)
        {
            if (!id.IsValidPartitionOrRowKey())
            {
                return null;
            }
            var partitionKey = ApplicationEntity.GeneratePartitionKey();
            var rowKey = ApplicationEntity.GenerateRowKey(id);

            var application = await _applicationTablestorage.GetDataAsync(partitionKey, rowKey);
            return Core.Application.Application.Create(application);
        }
    }
}
