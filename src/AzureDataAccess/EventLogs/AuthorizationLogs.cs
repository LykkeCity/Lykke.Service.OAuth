using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AzureStorage;
using Core.EventLogs;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureDataAccess.EventLogs
{
    public class AuthorizationLogRecordEntity : TableEntity, IAuthorizationLogRecord
    {
        public string Email { get; set; }
        public DateTime DateTime { get; set; }
        public string UserAgent { get; set; }

        public static string GeneratePartitionKey(string email)
        {
            return email;
        }

        public static AuthorizationLogRecordEntity Create(IAuthorizationLogRecord src)
        {
            return new AuthorizationLogRecordEntity
            {
                PartitionKey = GeneratePartitionKey(src.Email),
                DateTime = src.DateTime,
                Email = src.Email,
                UserAgent = src.UserAgent
            };
        }
    }

    public class AuthorizationLogsRepository : IAuthorizationLogsRepository
    {
        private readonly INoSQLTableStorage<AuthorizationLogRecordEntity> _tableStorage;

        public AuthorizationLogsRepository(INoSQLTableStorage<AuthorizationLogRecordEntity> tableStorage)
        {
            _tableStorage = tableStorage;
        }

        public Task AddRecordAsync(IAuthorizationLogRecord record)
        {
            var newEntity = AuthorizationLogRecordEntity.Create(record);

            return _tableStorage.InsertAndGenerateRowKeyAsDateTimeAsync(newEntity, record.DateTime);
        }

        public async Task<IEnumerable<IAuthorizationLogRecord>> GetAsync(string email, DateTime @from, DateTime to)
        {
            var partitionKey = AuthorizationLogRecordEntity.GeneratePartitionKey(email);

            return
                await _tableStorage.WhereAsync(partitionKey, @from.Date, to.Date.AddDays(1), ToIntervalOption.ExcludeTo);
        }
    }
}