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

        public TokenService(IConnectionMultiplexer connectionMultiplexer)
        {
            _redisDatabase = connectionMultiplexer.GetDatabase();
        }

        /// <inheritdoc 
        public Task<bool> RevokeRefreshTokenAsync(string refreshToken)
        {
            var tokenRedisKey = GetRefreshTokenWhitelistRedisKey(refreshToken);

            return string.IsNullOrWhiteSpace(tokenRedisKey)
                ? Task.FromResult(true)
                : _redisDatabase.KeyDeleteAsync(tokenRedisKey);
        }

        /// <inheritdoc />
        public Task<bool> IsRefreshTokenInWhitelistAsync(string refreshToken)
        {
            var tokenRedisKey = GetRefreshTokenWhitelistRedisKey(refreshToken);
            return _redisDatabase.KeyExistsAsync(tokenRedisKey);
        }

        /// <inheritdoc />
        public async Task<bool> UpdateRefreshTokenInWhitelistAsync(string oldRefreshToken, string newRefreshToken)
        {
            if (string.IsNullOrWhiteSpace(newRefreshToken))
                return false;

            var newKey = GetRefreshTokenWhitelistRedisKey(newRefreshToken);

            // If token is generated upon authorization code exchange save it to redis.
            if (string.IsNullOrWhiteSpace(oldRefreshToken))
            {
                await _redisDatabase.StringSetAsync(newKey, true, TimeSpan.FromDays(30));
                return true;
            }

            // If we successfully exchanged refresh token,
            // then remove it from Redis only if we saved a new one.
            var oldKey = GetRefreshTokenWhitelistRedisKey(oldRefreshToken);
            var steps = new List<Task>();
            var transaction = _redisDatabase.CreateTransaction();
            steps.Add(transaction.KeyDeleteAsync(oldKey));
            steps.Add(transaction.StringSetAsync(newKey, true, TimeSpan.FromDays(30)));
            if (await transaction.ExecuteAsync())
            {
                await Task.WhenAll(steps);
                return true;
            }

            return false;
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
