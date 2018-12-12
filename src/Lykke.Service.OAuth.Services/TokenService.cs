using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Core.ExternalProvider.Exceptions;
using Core.Services;
using IdentityModel.Client;
using JetBrains.Annotations;
using StackExchange.Redis;

namespace Lykke.Service.OAuth.Services
{
    [UsedImplicitly]
    public class TokenService : ITokenService
    {
        private readonly IDatabase _redisDatabase;
        private const string RedisPrefixIroncladRefreshTokens = "OAuth:IroncladRefreshTokens";
        private static readonly TimeSpan RefreshTokenWhitelistLifetime = TimeSpan.FromDays(30);
        //TODO:@gafanasiev get lifetime dynamically.
        private static readonly TimeSpan IroncladRefreshTokenLifetime = TimeSpan.FromDays(30);
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IDiscoveryCache _discoveryCache;

        public TokenService(
            IConnectionMultiplexer connectionMultiplexer, 
            IHttpClientFactory httpClientFactory, 
            IDiscoveryCache discoveryCache)
        {
            _httpClientFactory = httpClientFactory;
            _discoveryCache = discoveryCache;
            _redisDatabase = connectionMultiplexer.GetDatabase();
        }

        /// <inheritdoc />
        public Task<bool> RevokeRefreshTokenAsync(string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
                return Task.FromResult(true);

            var tokenRedisKey = GetRefreshTokenWhitelistRedisKey(refreshToken);

            return _redisDatabase.KeyDeleteAsync(tokenRedisKey);
        }

        /// <inheritdoc />
        public Task<bool> IsRefreshTokenInWhitelistAsync(string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
                return Task.FromResult(false);

            var tokenRedisKey = GetRefreshTokenWhitelistRedisKey(refreshToken);
            return _redisDatabase.KeyExistsAsync(tokenRedisKey);
        }

        /// <inheritdoc />
        public async Task UpdateRefreshTokenInWhitelistAsync(string oldRefreshToken, string newRefreshToken)
        {
            var isOldTokenPresent = !string.IsNullOrWhiteSpace(oldRefreshToken);
            var isNewTokenPresent = !string.IsNullOrWhiteSpace(newRefreshToken);

            // Replace or add only if new token is issued.
            if (isNewTokenPresent)
            {
                var oldKey = GetRefreshTokenWhitelistRedisKey(oldRefreshToken);
                var newKey = GetRefreshTokenWhitelistRedisKey(newRefreshToken);

                // If token is generated upon authorization code exchange save it to redis.
                if (!isOldTokenPresent)
                {
                    await _redisDatabase.StringSetAsync(newKey, true, RefreshTokenWhitelistLifetime);
                }
                else
                    // If we successfully exchanged refresh token,
                    // then remove it from Redis only if we saved a new one.
                {
                    var steps = new List<Task>();
                    var transaction = _redisDatabase.CreateTransaction();
                    steps.Add(transaction.KeyDeleteAsync(oldKey));
                    steps.Add(transaction.StringSetAsync(newKey, true, RefreshTokenWhitelistLifetime));
                    if (await transaction.ExecuteAsync()) await Task.WhenAll(steps);
                }
            }
        }

        /// <inheritdoc />
        public Task SaveIroncladRefreshTokenAsync(string lykkeToken, string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(lykkeToken))
                throw new ArgumentNullException(nameof(lykkeToken));         

            if (string.IsNullOrWhiteSpace(refreshToken))
                throw new ArgumentNullException(nameof(refreshToken));

            var redisKey = GetIroncladRefreshTokensRedisKey(lykkeToken);

            return _redisDatabase.StringSetAsync(redisKey, refreshToken, IroncladRefreshTokenLifetime);
        }

        /// <inheritdoc />
        public async Task<string> GetIroncladRefreshTokenAsync(string lykkeToken)
        {
            if (string.IsNullOrWhiteSpace(lykkeToken))
                throw new ArgumentNullException(nameof(lykkeToken));         

            var redisKey = GetIroncladRefreshTokensRedisKey(lykkeToken);

            var ironcladRefreshToken = await _redisDatabase.StringGetAsync(redisKey);

            if (ironcladRefreshToken.HasValue)
                return ironcladRefreshToken;

            throw new TokenNotFoundException("Ironclad refresh token not found!");
        }

        /// <inheritdoc />
        public async Task<string> GetIroncladAccessTokenAsync(string lykkeToken)
        {
            if (string.IsNullOrWhiteSpace(lykkeToken))
                throw new ArgumentNullException(nameof(lykkeToken));

            var ironcladRefreshToken = await GetIroncladRefreshTokenAsync(lykkeToken);

            var httpClient = _httpClientFactory.CreateClient();

            var discoveryResponse = await _discoveryCache.GetAsync();

            if (discoveryResponse.IsError)
            {
                _discoveryCache.Refresh();
                throw discoveryResponse.Exception;
            }

            //TODO:@gafanasiev get from settings
            var tokenResponse = await httpClient.RequestRefreshTokenAsync(new RefreshTokenRequest
            {
                Address = discoveryResponse.TokenEndpoint,
                RefreshToken = ironcladRefreshToken,
                ClientId = "sample_mvc",
                ClientSecret = "secret"
            });

            if (tokenResponse.IsError)
            {
                throw new TokenNotFoundException(tokenResponse.Error);
            }

            await SaveIroncladRefreshTokenAsync(lykkeToken, tokenResponse.RefreshToken);

            return tokenResponse.AccessToken;
        }

        private static string GetRefreshTokenWhitelistRedisKey(string refreshToken)
        {
            return "OAuth:RefreshTokens:Whitelist:" + CreateMd5(refreshToken);
        }

        private static string CreateMd5(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            using (var md5 = MD5.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(input);
                var hash = md5.ComputeHash(bytes);

                return Convert.ToBase64String(hash);
            }
        }

        private static string GetIroncladRefreshTokensRedisKey(string lykkeToken)
        {
            if (string.IsNullOrWhiteSpace(lykkeToken))
                throw new ArgumentNullException(nameof(lykkeToken));      
            
            return $"{RedisPrefixIroncladRefreshTokens}:{lykkeToken}";
        }
    }
}
