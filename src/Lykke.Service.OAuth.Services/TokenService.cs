using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Core.ExternalProvider;
using Core.ExternalProvider.Exceptions;
using Core.ExternalProvider.Settings;
using Core.Services;
using IdentityModel.Client;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using StackExchange.Redis;
using Newtonsoft.Json;


namespace Lykke.Service.OAuth.Services
{
    [UsedImplicitly]
    public class TokenService : ITokenService
    {
        private readonly IDatabase _redisDatabase;
        private const string PrefixIroncladTokens = "OAuth:IroncladTokens";
        private const string TokenDataProtector = "TokenDataProtector";

        private static readonly TimeSpan RefreshTokenWhitelistLifetime = TimeSpan.FromDays(30);
        private readonly IdentityProviderSettings _ironcladAuth;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IDiscoveryCache _discoveryCache;
        private readonly IOpenIdTokensFactory _openIdTokensFactory;
        private readonly ISystemClock _clock;
        private readonly IDataProtector _dataProtector;

        public TokenService(
            IdentityProviderSettings ironcladAuth,
            IConnectionMultiplexer connectionMultiplexer,
            IHttpClientFactory httpClientFactory,
            IDiscoveryCache discoveryCache,
            ISystemClock clock,
            IDataProtectionProvider dataProtectionProvider, 
            IOpenIdTokensFactory openIdTokensFactory)
        {
            _ironcladAuth = ironcladAuth;
            _httpClientFactory = httpClientFactory;
            _discoveryCache = discoveryCache;
            _redisDatabase = connectionMultiplexer.GetDatabase();
            _clock = clock;
            _openIdTokensFactory = openIdTokensFactory;
            _dataProtector = dataProtectionProvider.CreateProtector(TokenDataProtector);
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

        public Task SaveIroncladTokensAsync(string lykkeToken, OpenIdTokens tokens)
        {
            var data = SerializeAndProtect(tokens);

            return _redisDatabase.StringSetAsync(GetIroncladTokensRedisKey(lykkeToken), data, RefreshTokenWhitelistLifetime);
        }

        public async Task<OpenIdTokens> GetIroncladTokens(string lykkeToken)
        {
            var serialized =  await _redisDatabase.StringGetAsync(GetIroncladTokensRedisKey(lykkeToken));

            if (serialized.HasValue && !string.IsNullOrWhiteSpace(serialized))
                return DeserializeAndUnprotect<OpenIdTokens>(serialized);

            throw new TokenNotFoundException("Ironclad tokens not found in Redis!");
        }

        public Task<bool> DeleteIroncladTokens(string lykkeToken)
        {
            return _redisDatabase.KeyDeleteAsync(GetIroncladTokensRedisKey(lykkeToken));
        }

        public async Task<OpenIdTokens> GetFreshIroncladTokens(string lykkeToken)
        {
            var tokens = await GetIroncladTokens(lykkeToken);

            if (_clock.UtcNow < tokens.ExpiresAt) 
                return tokens;

            tokens = await RefreshIroncladTokensAsync(tokens.RefreshToken);

            await SaveIroncladTokensAsync(lykkeToken, tokens);

            return tokens;
        }
        
        public async Task RevokeIroncladTokensAsync(OpenIdTokens tokens)
        {
            var httpClient = _httpClientFactory.CreateClient();

            var discoveryResponse = await _discoveryCache.GetAsync();

            if (discoveryResponse.IsError)
            {
                _discoveryCache.Refresh();
                throw new TokenNotFoundException(discoveryResponse.Error);
            }

            await httpClient.RevokeTokenAsync(new TokenRevocationRequest
            {
                Address = discoveryResponse.RevocationEndpoint,
                ClientId = _ironcladAuth.ClientId,
                ClientSecret = _ironcladAuth.ClientSecret,
                Token = tokens.RefreshToken
            });

            await httpClient.RevokeTokenAsync(new TokenRevocationRequest
            {
                Address = discoveryResponse.RevocationEndpoint,
                ClientId = _ironcladAuth.ClientId,
                ClientSecret = _ironcladAuth.ClientSecret,
                Token = tokens.AccessToken
            });
        }
        
        private async Task<OpenIdTokens> RefreshIroncladTokensAsync(string ironcladRefreshToken)
        {
            var httpClient = _httpClientFactory.CreateClient();

            var discoveryResponse = await _discoveryCache.GetAsync();

            if (discoveryResponse.IsError)
            {
                _discoveryCache.Refresh();
                throw new TokenNotFoundException(discoveryResponse.Error);
            }

            var tokenResponse = await httpClient.RequestRefreshTokenAsync(new RefreshTokenRequest
            {
                Address = discoveryResponse.TokenEndpoint,
                RefreshToken = ironcladRefreshToken,
                ClientId = _ironcladAuth.ClientId,
                ClientSecret = _ironcladAuth.ClientSecret
            });

            if (tokenResponse.IsError)
                throw new TokenNotFoundException(tokenResponse.Error);

            //TODO:@gafanasiev may be use tokenResponse.ExpiresIn for expiration if correct time is returned.
            return _openIdTokensFactory.CreateOpenIdTokens(
                tokenResponse.IdentityToken, 
                tokenResponse.AccessToken,
                tokenResponse.RefreshToken);
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

        private static string GetIroncladTokensRedisKey(string lykkeToken)
        {
            if (string.IsNullOrWhiteSpace(lykkeToken))
                throw new ArgumentNullException(nameof(lykkeToken));

            return $"{PrefixIroncladTokens}:{lykkeToken}";
        }

        private string SerializeAndProtect<T>(T value)
        {
            var serialized = JsonConvert.SerializeObject(value);

            return _dataProtector.Protect(serialized);
        }

        private T DeserializeAndUnprotect<T>(string value)
        {
            var unprotected = _dataProtector.Unprotect(value);

            return JsonConvert.DeserializeObject<T>(unprotected);
        }
    }
}
