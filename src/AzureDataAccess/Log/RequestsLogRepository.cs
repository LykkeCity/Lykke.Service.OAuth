using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AzureStorage;
using Core.EventLogs;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureDataAccess.Log
{
    public class RequestsLogRecord : TableEntity, IRequestsLogRecord
    {
        private const int MaxFieldSize = 1024*4;

        public DateTime DateTime { get; set; }
        public string Url { get; set; }
        public string Request { get; set; }

        public string Response { get; set; }
        public string UserAgent { get; set; }

        public static string GeneratePartitionKey(string userId)
        {
            return userId;
        }

        public static RequestsLogRecord Create(string userId, string url, string request, string response,
            string userAgent)
        {
            if (request?.Length > MaxFieldSize)
                request = request.Substring(0, MaxFieldSize);

            return new RequestsLogRecord
            {
                PartitionKey = GeneratePartitionKey(userId),
                Url = url,
                Request = request,
                Response = response,
                DateTime = DateTime.UtcNow,
                UserAgent = userAgent
            };
        }
    }

    public class RequestsLogRepository : IRequestsLogRepository
    {
        private readonly INoSQLTableStorage<RequestsLogRecord> _tableStorage;

        public RequestsLogRepository(INoSQLTableStorage<RequestsLogRecord> tableStorage)
        {
            _tableStorage = tableStorage;
        }

        public Task WriteAsync(string clientId, string url, string request, string response, string userAgent)
        {
            var newEntity = RequestsLogRecord.Create(clientId, url, request, response, userAgent);
            return _tableStorage.InsertAndGenerateRowKeyAsDateTimeAsync(newEntity, newEntity.DateTime);
        }

        public async Task<IEnumerable<IRequestsLogRecord>> GetRecords(string clientId, DateTime @from, DateTime to)
        {
            var partitionKey = RequestsLogRecord.GeneratePartitionKey(clientId);

            return await _tableStorage.WhereAsync(partitionKey, @from, to.Date.AddDays(1), ToIntervalOption.ExcludeTo);
        }
    }
}