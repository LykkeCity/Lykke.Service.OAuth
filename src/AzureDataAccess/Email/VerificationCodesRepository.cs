using System;
using System.Threading.Tasks;
using AzureStorage;
using Core.Email;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureDataAccess.Email
{
    public class VerificationCodeEntity: TableEntity, IVerificationCode
    {
        public static string GeneratePartitionKey()
        {
            return "RegisterVerificationCode";
        }

        public static string GenerateRowKey(string key)
        {
            return key;
        }

        public static string GenerateCode()
        {
            return Guid.NewGuid().ToString("N");
        }

        public string Email { get; set; }
        public int ResendCount { get; set; }
        public string Code { get; set; }
        public string Key => RowKey;
        public string Referer { get; set; }
        public string ReturnUrl { get; set; }

        public static VerificationCodeEntity Create(string email, string referer, string returnUrl)
        {
            var rowKey = Guid.NewGuid().ToString("N");
            var code = GenerateCode();

            return new VerificationCodeEntity
            {
                PartitionKey = GeneratePartitionKey(),
                RowKey = GenerateRowKey(rowKey),
                Code = code,
                Email = email,
                Referer = referer,
                ReturnUrl = returnUrl
            };
        }
    }

    public class VerificationCodesRepository : IVerificationCodesRepository
    {
        private readonly INoSQLTableStorage<VerificationCodeEntity> _tablestorage;

        public VerificationCodesRepository(INoSQLTableStorage<VerificationCodeEntity> tablestorage)
        {
            _tablestorage = tablestorage;
        }

        public async Task<IVerificationCode> AddCodeAsync(string email, string referer, string returnUrl)
        {
            await DeleteCodesAsync(email);

            var entity = VerificationCodeEntity.Create(email, referer, returnUrl);
            await _tablestorage.InsertOrReplaceAsync(entity);

            return entity;
        }

        public async Task<IVerificationCode> GetCodeAsync(string key)
        {
            return await _tablestorage.GetDataAsync(VerificationCodeEntity.GeneratePartitionKey(), VerificationCodeEntity.GenerateRowKey(key));
        }

        public async Task<IVerificationCode> UpdateCodeAsync(string key)
        {
            var code = await _tablestorage.MergeAsync(VerificationCodeEntity.GeneratePartitionKey(),
                VerificationCodeEntity.GenerateRowKey(key),
                entity =>
                {
                    entity.Code = VerificationCodeEntity.GenerateCode();
                    entity.ResendCount++;
                    return entity;
                }
            );

            return code;
        }

        public async Task DeleteCodesAsync(string email)
        {
            var existingCodes = await _tablestorage.GetDataAsync(VerificationCodeEntity.GeneratePartitionKey(),
                item => item.Email == email);

            foreach (var existingCode in existingCodes)
                await _tablestorage.DeleteIfExistAsync(VerificationCodeEntity.GeneratePartitionKey(),
                    VerificationCodeEntity.GenerateRowKey(existingCode.Key));
        }
    }
}
