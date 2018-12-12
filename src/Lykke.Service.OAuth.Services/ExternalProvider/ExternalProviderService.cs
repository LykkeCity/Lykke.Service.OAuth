using System;
using System.Threading.Tasks;
using Core.ExternalProvider;
using StackExchange.Redis;

namespace Lykke.Service.OAuth.Services.ExternalProvider
{
    public class ExternalProviderService : IExternalProviderService
    {
        private const string RedisPrefix = "OAuth:IroncladLykkeLogins";
        private readonly IDatabase _database;

        public ExternalProviderService(IConnectionMultiplexer connectionMultiplexer)
        {
            _database = connectionMultiplexer.GetDatabase();
        }

        public async Task<string> SaveLykkeUserIdForExternalLoginAsync(string lykkeUserId, TimeSpan ttl)
        {
            if (string.IsNullOrWhiteSpace(lykkeUserId))
                throw new ArgumentNullException(nameof(lykkeUserId));

            var guid = Guid.NewGuid().ToString();

            var redisKey = GetRedisKey(guid);
            await _database.StringSetAsync(redisKey, lykkeUserId, ttl);
            return guid;
        }

        public async Task<string> GetLykkeUserIdForExternalLoginAsync(string guid)
        {
            if (string.IsNullOrWhiteSpace(guid))
                throw new ArgumentNullException(nameof(guid));

            var redisKey = GetRedisKey(guid);

            var lykkeUserId = await _database.StringGetAsync(redisKey);

            if (!lykkeUserId.HasValue)
                return string.Empty;

            return lykkeUserId;
        }

        private string GetRedisKey(string guid)
        {
            if (string.IsNullOrWhiteSpace(guid))
                throw new ArgumentNullException(nameof(guid));

            return $"{RedisPrefix}:{guid}";
        }
    }
}
