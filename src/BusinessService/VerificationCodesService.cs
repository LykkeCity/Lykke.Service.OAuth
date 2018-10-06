using System;
using System.IO;
using System.Threading.Tasks;
using Core.VerificationCodes;
using MessagePack;
using MessagePack.Resolvers;
using StackExchange.Redis;

namespace BusinessService
{
    public class VerificationCodesService : IVerificationCodesService
    {
        private const string RedisPrefix = "OAuth:EmailConfirmationCodes:"; 
        private readonly IDatabase _database;
        private readonly TimeSpan _verificationCodesExpiration;

        public VerificationCodesService(
            IConnectionMultiplexer connectionMultiplexer,
            TimeSpan verificationCodesExpiration
            )
        {
            _database = connectionMultiplexer.GetDatabase();
            _verificationCodesExpiration = verificationCodesExpiration;
        }
        
        public async Task<VerificationCode> AddCodeAsync(string email, string referer, string returnUrl, string cid, string traffic)
        {
            var code = new VerificationCode(email, referer, returnUrl, cid, traffic);

            await AddCacheAsync(code);

            return code;
        }

        public async Task<VerificationCode> GetCodeAsync(string key)
        {
            var value = await _database.StringGetAsync(GetRedisKey(key));

            if (!value.HasValue)
                return null;
            
            using (var stream = new MemoryStream(value))
            {
                return MessagePackSerializer.Deserialize<VerificationCode>(stream, StandardResolverAllowPrivate.Instance);
            }
        }

        public async Task<VerificationCode> UpdateCodeAsync(string key)
        {
            var code = await GetCodeAsync(key);

            if (code == null) 
                return null;
            
            code.UpdateCode();
            await AddCacheAsync(code);

            return code;
        }

        public Task DeleteCodeAsync(string key)
        {
            return _database.KeyDeleteAsync(GetRedisKey(key));
        }

        private async Task AddCacheAsync(VerificationCode code)
        {
            var value = MessagePackSerializer.Serialize(code);

            await _database.StringSetAsync(GetRedisKey(code.Key), value, _verificationCodesExpiration);
        }

        private string GetRedisKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            return string.Concat(RedisPrefix, key);
        }
    }
}
