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
        private readonly IDatabase _redisDatabase;
        private readonly TimeSpan _verificationCodesExpiration;

        public VerificationCodesService(
            IConnectionMultiplexer connectionMultiplexer,
            TimeSpan verificationCodesExpiration
            )
        {
            _redisDatabase = connectionMultiplexer.GetDatabase() ?? throw new ArgumentNullException(nameof(connectionMultiplexer));
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
            var redisValue = await _redisDatabase.StringGetAsync(GetCacheKey(key));
            using (var stream = new MemoryStream(redisValue))
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
            return _redisDatabase.KeyDeleteAsync(GetCacheKey(key));
        }

        private async Task AddCacheAsync(VerificationCode code)
        {
            var value = MessagePackSerializer.Serialize(code);
            await _redisDatabase.StringSetAsync(GetCacheKey(code.Key), value, _verificationCodesExpiration);
        }
        private static string GetCacheKey(string key)
        {
            return "OAuth:ConfirmationCodes:" + key;
        }
    }
}
