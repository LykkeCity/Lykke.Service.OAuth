using System;
using System.Threading.Tasks;
using Core.Exceptions;
using Core.ExternalProvider;
using StackExchange.Redis;

namespace Lykke.Service.OAuth.Services.ExternalProvider
{
    public class ExternalUserService : IExternalUserService
    {
        private const string RedisPrefix = "OAuth:ExternalUserBindings"; 
        private readonly IDatabase _database;

        public ExternalUserService(IConnectionMultiplexer connectionMultiplexer)
        {
            _database = connectionMultiplexer.GetDatabase();
        }

        public Task AssociateExternalUserAsync(string provider, string externalUserId, string lykkeUserId)
        {
            if (string.IsNullOrWhiteSpace(provider))
                throw new ArgumentNullException(nameof(provider));
            
            if (string.IsNullOrWhiteSpace(lykkeUserId))
                throw new ArgumentNullException(nameof(lykkeUserId));     
            
            if (string.IsNullOrWhiteSpace(externalUserId))
                throw new ArgumentNullException(nameof(externalUserId));

            var redisKey = GetRedisKey(provider, externalUserId);

            if(_database.KeyExists(redisKey))
                throw new ExternalUserAlreadyAssociatedException("User account already associated!");

            return _database.StringSetAsync(redisKey, lykkeUserId);
        }

        
        public async Task<string> GetAssociatedLykkeUserIdAsync(string provider, string externalUserId)
        {
            if (string.IsNullOrWhiteSpace(provider))
                throw new ArgumentNullException(nameof(provider));         
            
            if (string.IsNullOrWhiteSpace(externalUserId))
                throw new ArgumentNullException(nameof(externalUserId));

            var redisKey = GetRedisKey(provider, externalUserId);

            var lykkeUserId = await _database.StringGetAsync(redisKey);

            if (!lykkeUserId.HasValue)
                return string.Empty;

            return lykkeUserId;
        }

        private string GetRedisKey(string provider, string externalUserId)
        {
            if (string.IsNullOrWhiteSpace(provider))
                throw new ArgumentNullException(nameof(provider));
            
            if (string.IsNullOrWhiteSpace(externalUserId))
                throw new ArgumentNullException(nameof(externalUserId));

            return $"{RedisPrefix}:{provider}:{externalUserId}";
        }
    }
}
