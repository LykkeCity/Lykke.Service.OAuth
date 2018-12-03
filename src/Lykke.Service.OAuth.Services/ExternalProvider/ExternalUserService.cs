using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Core.Exceptions;
using Core.ExternalProvider;
using Core.ExternalProvider.Exceptions;
using IdentityModel;
using IdentityModel.Client;
using Ironclad.Client;
using Lykke.Service.ClientAccount.Client;
using Lykke.Service.ClientAccount.Client.AutorestClient.Models;
using Lykke.Service.ClientAccount.Client.Models;
using Lykke.Service.PersonalData.Client.Models;
using Lykke.Service.PersonalData.Contract;
using Lykke.Service.PersonalData.Contract.Models;
using StackExchange.Redis;

namespace Lykke.Service.OAuth.Services.ExternalProvider
{
    public class ExternalUserService : IExternalUserService
    {
        private const string RedisPrefixExternalUserBindings = "OAuth:ExternalUserBindings";
        private const string RedisPrefixIroncladLykkeLogins = "OAuth:IroncladLykkeLogins";

        private readonly IDatabase _database;
        private readonly IClientAccountClient _clientAccountClient;
        private readonly IPersonalDataService _personalDataService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IDiscoveryCache _discoveryCache;


        public ExternalUserService(
            IConnectionMultiplexer connectionMultiplexer,
            IClientAccountClient clientAccountClient,
            IPersonalDataService personalDataService,
            IHttpClientFactory httpClientFactory, IDiscoveryCache discoveryCache)
        {
            _database = connectionMultiplexer.GetDatabase();
            _clientAccountClient = clientAccountClient;
            _personalDataService = personalDataService;
            _httpClientFactory = httpClientFactory;
            _discoveryCache = discoveryCache;
        }

        /// <inheritdoc />
        public Task AssociateExternalUserAsync(string provider, string externalUserId, string lykkeUserId)
        {
            if (string.IsNullOrWhiteSpace(provider))
                throw new ArgumentNullException(nameof(provider));

            if (string.IsNullOrWhiteSpace(lykkeUserId))
                throw new ArgumentNullException(nameof(lykkeUserId));

            if (string.IsNullOrWhiteSpace(externalUserId))
                throw new ArgumentNullException(nameof(externalUserId));

            var redisKey = GetExternalUserBindingsRedisKey(provider, externalUserId);

            if (_database.KeyExists(redisKey))
                throw new ExternalUserAlreadyAssociatedException("User account already associated!");

            return _database.StringSetAsync(redisKey, lykkeUserId);
        }

        public async Task AddClaimToIroncladUser(string ironcladUserId, string type, object value)
        {
            var httpClient = _httpClientFactory.CreateClient();
            const string authority = "http://localhost:5005";
            
            var discoveryResponse = await _discoveryCache.GetAsync();

            //TODO:@gafanasiev Add error handling.
            //if (!discoveryResponse.IsError)
            //{

            //}

            var tokenResponse = await httpClient.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = discoveryResponse.TokenEndpoint,
                ClientId = "sample_mvc",
                ClientSecret = "secret",
            });

            using (var tokenClient = new TokenClient(discoveryResponse.TokenEndpoint, "auth_console"))
            using (var refreshTokenHandler = new RefreshTokenDelegatingHandler(tokenClient, tokenResponse.RefreshToken, tokenResponse.AccessToken) { InnerHandler = new HttpClientHandler() })
            using (var usersClient = new UsersHttpClient(authority, refreshTokenHandler))
            {
                var claims = new Dictionary<string, object>
                {
                    {type, value}
                };

                await usersClient.ModifyUserAsync(new User
                {
                    Id = ironcladUserId,
                    Roles = null,
                    Claims = claims
                });
            }

            //TODO:@gafanasiev Add error handling.
            //if (tokenResponse.IsError)
            //{
            //    throw 
            //}
        } 

        /// <inheritdoc />
        public async Task<string> GetAssociatedLykkeUserIdAsync(string provider, string externalUserId)
        {
            if (string.IsNullOrWhiteSpace(provider))
                throw new ArgumentNullException(nameof(provider));

            if (string.IsNullOrWhiteSpace(externalUserId))
                throw new ArgumentNullException(nameof(externalUserId));

            var redisKey = GetExternalUserBindingsRedisKey(provider, externalUserId);

            var lykkeUserId = await _database.StringGetAsync(redisKey);

            if (!lykkeUserId.HasValue)
                return string.Empty;

            return lykkeUserId;
        }

        /// <inheritdoc />
        public async Task<ClientAccountInformationModel> ProvisionIfNotExistAsync(ClaimsPrincipal principal)
        {
            var externalUserId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(externalUserId))
                throw new ExternalProviderClaimNotFoundException("Claim for external user id not found.");

            var idp = principal.FindFirst(JwtClaimTypes.IdentityProvider)?.Value;

            if (string.IsNullOrWhiteSpace(idp))
                throw new ExternalProviderClaimNotFoundException("Claim for idp not found.");

            var existingAccount = await _clientAccountClient.GetClientByExternalIdentityProvider(
                idp,
                externalUserId);

            if (existingAccount != null)
                return existingAccount;

            var email = principal.FindFirst(JwtClaimTypes.Email)?.Value;

            if (string.IsNullOrWhiteSpace(email))
                throw new ExternalProviderClaimNotFoundException(JwtClaimTypes.Email,
                    idp,
                    externalUserId);

            var isEmailVerified = principal.FindFirst(JwtClaimTypes.EmailVerified)?.Value;

            if (string.IsNullOrWhiteSpace(isEmailVerified))
                throw new ExternalProviderClaimNotFoundException(JwtClaimTypes.EmailVerified,
                    idp,
                    externalUserId);

            if (!Convert.ToBoolean(isEmailVerified))
                throw new ExternalProviderPhoneNotVerifiedException("Email is not verified on provider side!");

            var phone = principal.FindFirst(JwtClaimTypes.PhoneNumber)?.Value;

            if (string.IsNullOrWhiteSpace(phone))
                throw new ExternalProviderClaimNotFoundException(JwtClaimTypes.PhoneNumber,
                    idp,
                    externalUserId);

            var isPhoneVerified = principal.FindFirst(JwtClaimTypes.PhoneNumberVerified)?.Value;

            if (string.IsNullOrWhiteSpace(isPhoneVerified))
                throw new ExternalProviderClaimNotFoundException(JwtClaimTypes.PhoneNumberVerified,
                    idp,
                    externalUserId);

            if (!Convert.ToBoolean(isPhoneVerified))
                throw new ExternalProviderPhoneNotVerifiedException("Phone is not verified on provider side!");

            var newAccount =
                await _clientAccountClient.ProvisionAsync(new ExternalClientProvisionModel
                {
                    Email = email,
                    ExternalIdentityProviderId = idp,
                    ExternalUserId = externalUserId,
                    Phone = phone
                });

            if (newAccount == null)
                throw new UserAutoprovisionFailedException("User autoprovision failed!");

            var claims = principal.Claims.Select(claim => new IdentityProviderOriginalClaim
            {
                Name = claim.Type,
                Value = claim.Value
            });

            await _personalDataService.SaveClaimsAsync(new IdentityProviderClaimsModel
            {
                ExternalProviderId = idp,
                ExternalUserId = externalUserId,
                LykkeClientId = newAccount.Id,
                EmailClaim = newAccount.Email,
                PhoneClaim = newAccount.Phone,
                OriginalClaims = claims
            });

            return newAccount;
        }

        /// <inheritdoc />
        public async Task<string> SaveLykkeUserIdForExternalLoginAsync(string lykkeUserId, TimeSpan ttl)
        {
            if (string.IsNullOrWhiteSpace(lykkeUserId))
                throw new ArgumentNullException(nameof(lykkeUserId));

            var guid = Guid.NewGuid().ToString();

            var redisKey = GetIroncladLykkeLoginsRedisKey(guid);
            await _database.StringSetAsync(redisKey, lykkeUserId, ttl);
            return guid;
        }

        /// <inheritdoc />
        public async Task<string> GetLykkeUserIdForExternalLoginAsync(string guid)
        {
            if (string.IsNullOrWhiteSpace(guid))
                throw new ArgumentNullException(nameof(guid));

            var redisKey = GetIroncladLykkeLoginsRedisKey(guid);

            var lykkeUserId = await _database.StringGetAsync(redisKey);

            if (!lykkeUserId.HasValue)
                return string.Empty;

            return lykkeUserId;
        }

        private string GetExternalUserBindingsRedisKey(string provider, string externalUserId)
        {
            if (string.IsNullOrWhiteSpace(provider))
                throw new ArgumentNullException(nameof(provider));

            if (string.IsNullOrWhiteSpace(externalUserId))
                throw new ArgumentNullException(nameof(externalUserId));

            return $"{RedisPrefixExternalUserBindings}:{provider}:{externalUserId}";
        }

        private string GetIroncladLykkeLoginsRedisKey(string guid)
        {
            if (string.IsNullOrWhiteSpace(guid))
                throw new ArgumentNullException(nameof(guid));

            return $"{RedisPrefixIroncladLykkeLogins}:{guid}";
        }
    }
}
