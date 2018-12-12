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

        //TODO:@gafanasiev Remove
        private static readonly Dictionary<string, ExternalIdentityProvider> ExternalProviders =
            new Dictionary<string, ExternalIdentityProvider>();

        private static readonly Dictionary<string, string> IssToProviderId = new Dictionary<string, string>();

        public ExternalProviderService(
            IEnumerable<ExternalIdentityProvider> externalIdentityProviders,
            IConnectionMultiplexer connectionMultiplexer)
        {
            _database = connectionMultiplexer.GetDatabase();

            foreach (var provider in externalIdentityProviders)
            {
                if (provider == null) continue;
                
                ExternalProviders.Add(provider.Id, provider);

                foreach (var iss in provider.ValidIssuers) 
                    IssToProviderId.Add(iss, provider.Id);
            }
        }

        /// <inheritdoc/>
        public async Task<string> SaveLykkeUserIdForExternalLoginAsync(string lykkeUserId, TimeSpan ttl)
        {
            if (string.IsNullOrWhiteSpace(lykkeUserId))
                throw new ArgumentNullException(nameof(lykkeUserId));

            var guid = Guid.NewGuid().ToString();

            var redisKey = GetRedisKey(guid);
            await _database.StringSetAsync(redisKey, lykkeUserId, ttl);
            return guid;
        }

        /// <inheritdoc/>
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

        
        /// <inheritdoc/>
        public string GetProviderId(string iss)
        {
            var notFoundException = new ExternalProviderNotFoundException($"Provider id not found by specified issuer: {iss}");

            if (string.IsNullOrWhiteSpace(iss))
                throw notFoundException;

            if (IssToProviderId.TryGetValue(iss, out var providerId)) return providerId;

            throw notFoundException;
        }

        /// <inheritdoc/>
        public ExternalIdentityProvider GetProviderConfiguration(string providerId)
        {
            var notFoundException = new ExternalProviderNotFoundException($"Provider not found by specified providerId: {providerId}");
            
            if (string.IsNullOrWhiteSpace(providerId))
                throw notFoundException;
            
            if (ExternalProviders.TryGetValue(providerId, out var provider)) return provider;

            throw notFoundException;
        }

        private string GetRedisKey(string guid)
        {
            if (string.IsNullOrWhiteSpace(guid))
                throw new ArgumentNullException(nameof(guid));

            return $"{RedisPrefix}:{guid}";
        }
    }
}
