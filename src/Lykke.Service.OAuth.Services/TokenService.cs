using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Core.Services;
using JetBrains.Annotations;
using StackExchange.Redis;

namespace Lykke.Service.OAuth.Services
{
    [UsedImplicitly]
    public class TokenService : ITokenService
    {
        private readonly IDatabase _redisDatabase;
        private static readonly TimeSpan RefreshTokenWhitelistLifetime = TimeSpan.FromDays(30);

        public TokenService(IConnectionMultiplexer connectionMultiplexer)
        {
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

            var oldKey = GetRefreshTokenWhitelistRedisKey(oldRefreshToken);
            var newKey = GetRefreshTokenWhitelistRedisKey(newRefreshToken);

            // Replace or add only if new token is issued.
            if (isNewTokenPresent)
            {
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
    }
}
