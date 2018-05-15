using System;
using System.IO;
using System.Threading.Tasks;
using Core.VerificationCodes;
using MessagePack;
using MessagePack.Resolvers;
using Microsoft.Extensions.Caching.Distributed;

namespace BusinessService
{
    public class VerificationCodesService : IVerificationCodesService
    {
        private readonly IDistributedCache _cache;
        private readonly TimeSpan _verificationCodesExpiration;

        public VerificationCodesService(
            IDistributedCache cache,
            TimeSpan verificationCodesExpiration
            )
        {
            _cache = cache;
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
            var value = await _cache.GetAsync(key);

            if (value == null)
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
            return _cache.RemoveAsync(key);
        }

        private async Task AddCacheAsync(VerificationCode code)
        {
            var value = MessagePackSerializer.Serialize(code);

            await _cache.SetAsync(code.Key, value, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = _verificationCodesExpiration
            });
        }
    }
}
