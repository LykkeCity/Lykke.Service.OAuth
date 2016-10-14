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

        public static ApplicationEntity Create(IApplication application)
        {
            return new ApplicationEntity
            {
                PartitionKey = GeneratePartitionKey(),
                RowKey = Guid.NewGuid().ToString(),
                DisplayName = application.DisplayName,
                RedirectUri = application.RedirectUri,
                Secret = Guid.NewGuid().ToString()
            };
        }

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

        public async Task<IEnumerable<IApplication>> GetApplicationsAsync()
        {
            var partitionKey = ApplicationEntity.GeneratePartitionKey();
            return await _applicationTablestorage.GetDataAsync(partitionKey);
        }

        public Task RegisterApplicationAsync(IApplication application)
        {
            var newApplication = ApplicationEntity.Create(application);
            return _applicationTablestorage.InsertAsync(newApplication);
        }

        public async Task EditApplicationAsync(string id, IApplication application)
        {
            await
                _applicationTablestorage.ReplaceAsync(ApplicationEntity.GeneratePartitionKey(),
                    ApplicationEntity.GenerateRowKey(id),
                    applicationEntity =>
                    {
                        applicationEntity.DisplayName = application.DisplayName;
                        applicationEntity.RedirectUri = application.RedirectUri;
                        return applicationEntity;
                    });
        }

        public async Task DeleteAsync(string id)
        {
            var partitionKey = ApplicationEntity.GeneratePartitionKey();
            var rowKey = ApplicationEntity.GenerateRowKey(id);
            await _applicationTablestorage.DeleteAsync(partitionKey, rowKey);
        }
    }
}