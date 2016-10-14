using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AzureStorage;
using Core.EventLogs;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureDataAccess.EventLogs
{
    public class RegistrationLogEventEntity : TableEntity, IRegistrationLogEvent
    {
        public DateTime DateTime { get; set; }
        public string ClientId { get; set; }
        public string Id => RowKey;
        public string Email { get; set; }
        public string FullName { get; set; }
        public string ContactPhone { get; set; }
        public string DeviceInfo { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public string Ip { get; set; }
        public string Isp { get; set; }

        public static string GeneratePartitionKey()
        {
            return "RegEvnt";
        }

        public static RegistrationLogEventEntity Create(IRegistrationLogEvent src)
        {
            return new RegistrationLogEventEntity
            {
                PartitionKey = GeneratePartitionKey(),
                DateTime = src.DateTime,
                ClientId = src.ClientId,
                DeviceInfo = src.DeviceInfo,
                ContactPhone = src.ContactPhone,
                Country = src.Country,
                Email = src.Email,
                FullName = src.FullName,
                Ip = src.Ip,
                City = src.City,
                Isp = src.Isp
            };
        }
    }

    public class RegistrationLogs : IRegistrationLogs
    {
        private readonly INoSQLTableStorage<RegistrationLogEventEntity> _tableStorage;

        public RegistrationLogs(INoSQLTableStorage<RegistrationLogEventEntity> tableStorage)
        {
            _tableStorage = tableStorage;
        }

        public async Task<IRegistrationLogEvent> RegisterEventAsync(IRegistrationLogEvent evnt)
        {
            var newEntity = RegistrationLogEventEntity.Create(evnt);

            return await _tableStorage.InsertAndGenerateRowKeyAsDateTimeAsync(newEntity, evnt.DateTime);
        }

        public async Task<IEnumerable<IRegistrationLogEvent>> GetAsync(DateTime @from, DateTime to)
        {
            var partitionKey = RegistrationLogEventEntity.GeneratePartitionKey();

            return
                await _tableStorage.WhereAsync(partitionKey, @from.Date, to.Date.AddDays(1), ToIntervalOption.ExcludeTo);
        }

        public Task UpdateGeolocationDataAsync(string id, string countryCode, string city, string isp)
        {
            var partitionKey = RegistrationLogEventEntity.GeneratePartitionKey();

            return _tableStorage.ReplaceAsync(partitionKey, id, entity =>
            {
                entity.City = city;
                entity.Country = countryCode;
                entity.Isp = isp;
                return entity;
            });
        }
    }
}