using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AzureStorage;
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

        public async Task<IApplication> GetByIdAsync(string id)
        {
            var partitionKey = ApplicationEntity.GeneratePartitionKey();
            var rowKey = ApplicationEntity.GenerateRowKey(id);

            return await _applicationTablestorage.GetDataAsync(partitionKey, rowKey);
        }
    }
}
