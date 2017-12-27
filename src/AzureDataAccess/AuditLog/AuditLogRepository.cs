using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage;
using Common;
using Core.AuditLog;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureDataAccess.AuditLog
{
    public class AuditLogDataEntity : TableEntity, IAuditLogData
    {
        public static string GenerateRowKey(DateTime creationDt)
        {
            return IdGenerator.GenerateDateTimeIdNewFirst(creationDt);
        }

        public static string GeneratePartitionKey(string clientId, AuditRecordType recordType)
        {
            return $"AUD_{clientId}_{(int)recordType}";
        }

        public DateTime CreatedTime { get; set; }

        public AuditRecordType RecordType
        {
            get
            {
                var parts = PartitionKey.Split('_');
                return (AuditRecordType)int.Parse(parts.Last());
            }
        }

        public string EventRecord { get; set; }

        public string BeforeJson { get; set; }
        public string AfterJson { get; set; }
        public string Changer { get; set; }

        public static AuditLogDataEntity Create(string clientId, IAuditLogData data)
        {
            return new AuditLogDataEntity
            {
                RowKey = GenerateRowKey(data.CreatedTime),
                PartitionKey = GeneratePartitionKey(clientId, data.RecordType),
                CreatedTime = data.CreatedTime,
                BeforeJson = data.BeforeJson,
                AfterJson = data.AfterJson,
                EventRecord = data.EventRecord,
                Changer = data.Changer
            };
        }

    }

    public class AuditLogRepository : IAuditLogRepository
    {
        private INoSQLTableStorage<AuditLogDataEntity> _tableStorage;

        public AuditLogRepository(INoSQLTableStorage<AuditLogDataEntity> tableStorage)
        {
            _tableStorage = tableStorage;
        }

        public async Task InsertRecord(string clientId, IAuditLogData record)
        {
            var entity = AuditLogDataEntity.Create(clientId, record);
            await _tableStorage.InsertAsync(entity);
        }

        public async Task<IEnumerable<IAuditLogData>> GetKycRecordsAsync(string clientId)
        {
            var records = new List<AuditLogDataEntity>();
            var kycDocumentChangesTask =
                _tableStorage.GetDataAsync(AuditLogDataEntity.GeneratePartitionKey(clientId, AuditRecordType.KycDocument));
            var kycStatusChangesTask =
                _tableStorage.GetDataAsync(AuditLogDataEntity.GeneratePartitionKey(clientId, AuditRecordType.KycStatus));
            var kycPersonalDataTask =
                _tableStorage.GetDataAsync(AuditLogDataEntity.GeneratePartitionKey(clientId, AuditRecordType.PersonalData));
            var otherEventRecordsTask =
                _tableStorage.GetDataAsync(AuditLogDataEntity.GeneratePartitionKey(clientId, AuditRecordType.OtherEvent));

            records.AddRange(await kycDocumentChangesTask);
            records.AddRange(await kycStatusChangesTask);
            records.AddRange(await kycPersonalDataTask);
            records.AddRange(await otherEventRecordsTask);

            return records.OrderByDescending(x => x.CreatedTime);
        }
    }
}
