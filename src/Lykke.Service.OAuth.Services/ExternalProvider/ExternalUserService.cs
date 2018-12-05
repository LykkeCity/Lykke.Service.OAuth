using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Core.Exceptions;
using Core.Extensions;
using Core.ExternalProvider;
using Core.ExternalProvider.Exceptions;
using Core.Services;
using IdentityModel;
using IdentityModel.Client;
using Ironclad.Client;
using Lykke.Service.ClientAccount.Client;
using Lykke.Service.ClientAccount.Client.AutorestClient.Models;
using Lykke.Service.ClientAccount.Client.Models;
using Lykke.Service.PersonalData.Client.Models;
using Lykke.Service.PersonalData.Contract;
using Lykke.Service.PersonalData.Contract.Models;
using Lykke.Service.Session.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using StackExchange.Redis;

namespace Lykke.Service.OAuth.Services.ExternalProvider
{
    public class ExternalUserService : IExternalUserService
    {
        private const string RedisPrefixExternalUserBindings = "OAuth:ExternalUserBindings";
        private const string RedisPrefixIroncladLykkeLogins = "OAuth:IroncladLykkeLogins";

        private readonly TimeSpan _ironcladLykkeLoginsLifetime = TimeSpan.FromMinutes(3);
        private readonly TimeSpan _mobileSessionLifetime = TimeSpan.FromDays(30);
        
        private readonly IDatabase _database;
        private readonly IClientAccountClient _clientAccountClient;
        private readonly IPersonalDataService _personalDataService;
        private readonly IDataProtector _dataProtector;
        private readonly IClientSessionsClient _clientSessionsClient;
        private readonly ITokenService _tokenService;
        private readonly IIroncladService _ironcladService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        
        public ExternalUserService(
            IConnectionMultiplexer connectionMultiplexer,
            IClientAccountClient clientAccountClient,
            IPersonalDataService personalDataService,
            IDataProtectionProvider dataProtectionProvider,
            IHttpContextAccessor httpContextAccessor,
            IClientSessionsClient clientSessionsClient,
            ITokenService tokenService, 
            IIroncladService ironcladService)
        {
            _database = connectionMultiplexer.GetDatabase();
            _clientAccountClient = clientAccountClient;
            _personalDataService = personalDataService;
            _httpContextAccessor = httpContextAccessor;
            _clientSessionsClient = clientSessionsClient;
            _tokenService = tokenService;
            _ironcladService = ironcladService;
            _dataProtector =
                dataProtectionProvider.CreateProtector(OpenIdConnectConstantsExt.Protectors.ExternalProviderCookieProtector);
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

        /// <inheritdoc />
        public async Task<string> GetAssociatedLykkeUserIdAsync(string provider, string externalUserId)
        {
            if (string.IsNullOrWhiteSpace(provider))
                throw new ArgumentNullException(nameof(provider));

            if (string.IsNullOrWhiteSpace(externalUserId))
                throw new ArgumentNullException(nameof(externalUserId));

            var redisKey = GetExternalUserBindingsRedisKey(provider, externalUserId);

            var lykkeUserId = await _database.StringGetAsync(redisKey);

            //TODO:@gafanasiev Throw?
            if (lykkeUserId.HasValue)
                return lykkeUserId;

            return string.Empty;
        }

        private async Task<ClientAccountInformationModel> ProvisionIfNotExistAsync(ClaimsPrincipal principal)
        {
            var externalUserId = principal.GetTokenClaim(ClaimTypes.NameIdentifier);

            var idp = principal.GetTokenClaim("http://schemas.microsoft.com/identity/claims/identityprovider");

            var existingLykkeUser = await _clientAccountClient.GetClientByExternalIdentityProvider(
                idp,
                externalUserId);

            if (existingLykkeUser != null)
                return existingLykkeUser;

            var email = principal.GetTokenClaim(JwtClaimTypes.Email);

            var isEmailVerified = principal.GetTokenClaim(JwtClaimTypes.EmailVerified);

            if (!Convert.ToBoolean(isEmailVerified))
                throw new AuthenticationException("Email is not verified on provider side!");

            var phone = principal.GetTokenClaim(JwtClaimTypes.PhoneNumber);

            var isPhoneVerified = principal.GetTokenClaim(JwtClaimTypes.PhoneNumberVerified);

            if (!Convert.ToBoolean(isPhoneVerified))
                throw new AuthenticationException("Phone is not verified on provider side!");

            var newLykkeUser =
                await _clientAccountClient.ProvisionAsync(new ExternalClientProvisionModel
                {
                    Email = email,
                    ExternalIdentityProviderId = idp,
                    ExternalUserId = externalUserId,
                    Phone = phone
                });

            if (newLykkeUser == null)
                throw new AutoprovisionException($"Could not provision external user, idp:{idp}, externalUserId:{externalUserId}");

            var lykkeUserId = newLykkeUser.Id;

            var claims = principal.Claims.Select(claim => new IdentityProviderOriginalClaim
            {
                Name = claim.Type,
                Value = claim.Value
            });

            await _personalDataService.SaveClaimsAsync(new IdentityProviderClaimsModel
            {
                ExternalProviderId = idp,
                ExternalUserId = externalUserId,
                LykkeClientId = newLykkeUser.Id,
                EmailClaim = newLykkeUser.Email,
                PhoneClaim = newLykkeUser.Phone,
                OriginalClaims = claims
            });

            await AssociateExternalUserAsync(
                idp,
                externalUserId,
                lykkeUserId);

            await _ironcladService.AddClaim(externalUserId, OpenIdConnectConstantsExt.Claims.Lsub, lykkeUserId);

            return newLykkeUser;
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

            //TODO:@gafanasiev Throw?
            if (lykkeUserId.HasValue)
                return lykkeUserId;

            return string.Empty;
        }

        public async Task<string> GetLykkeUserIdFromCookieAsync()
        {
            /* If user authenticated through Lykke OAuth on Ironclad side.
            * But not associated, get lykkeUserId from cookie and associate user.
            */
            var guidExists = _httpContextAccessor.HttpContext.Request.Cookies.TryGetValue(
                OpenIdConnectConstantsExt.Cookies.TemporaryUserIdCookie,
                out var protectedGuid);

            /* Cookie could be empty if user is already authenticated in Ironclad.
             * This means Ironclad would not redirect to Lykke OAuth but immediately return authenticated user.
             * Thus cookie would not be created during login.
             */
            if (!guidExists || string.IsNullOrWhiteSpace(protectedGuid))
            {
                throw new AuthenticationException("Authenticated through Lykke, but guid is not saved to cookie.");
            }

            var guid = _dataProtector.Unprotect(protectedGuid);

            var lykkeUserId = await GetLykkeUserIdForExternalLoginAsync(guid);

            if (string.IsNullOrWhiteSpace(lykkeUserId))
            {
                throw new AuthenticationException($"Authenticated through Lykke, but lykkeUserId was not found for guid:{guid}.");
            }

            return lykkeUserId;
        }

        public async Task<LykkeUserAuthenticationContext> HandleExternalUserLogin(ClaimsPrincipal principal)
        {
            var externalUserId = principal.GetTokenClaim(JwtClaimTypes.Subject);

            var identityProvider = principal.GetTokenClaim("http://schemas.microsoft.com/identity/claims/identityprovider");

            ClientAccountInformationModel lykkeUser = null;

            //Try to find id in lsub.
            var lykkeUserId = principal.FindFirst(OpenIdConnectConstantsExt.Claims.Lsub)?.Value;

            // If user does not have lsub claim.
            // Check if external user is already associated with Lykke user.
            if (string.IsNullOrWhiteSpace(lykkeUserId))
            {
                lykkeUserId =
                    await GetAssociatedLykkeUserIdAsync(
                        identityProvider,
                        externalUserId);
            }

            var shouldAssociateUser = string.IsNullOrWhiteSpace(lykkeUserId);

            if (shouldAssociateUser)
            {
                if (identityProvider.Equals(OpenIdConnectConstantsExt.Providers.Lykke))
                {
                    lykkeUserId = await GetLykkeUserIdFromCookieAsync();
                    lykkeUser = await _clientAccountClient.GetClientByIdAsync(lykkeUserId);
                }
                else
                {
                    lykkeUser = await ProvisionIfNotExistAsync(principal);
                }
            }

            // Check if lykke user exists.

            if (lykkeUser == null)
            {
                throw new AuthenticationException($"Lykke user with id:{lykkeUserId} does not exist.");
            }

            // We must be sure that user exists before associating it.
            if(shouldAssociateUser)
                await AssociateExternalUserAsync(
                    identityProvider,
                    externalUserId,
                    lykkeUserId);

            //TODO:@gafanasiev Think how to get already created session and use it.
            var clientSession =
                await _clientSessionsClient.Authenticate(lykkeUserId, string.Empty, null, null,
                    _mobileSessionLifetime);

            if (clientSession == null)
                throw new AuthenticationException($"Unable to create client session! ClientId: {lykkeUserId}");

            var sessionId = clientSession.SessionToken;

            var refreshToken = await _httpContextAccessor.HttpContext.GetTokenAsync(
                OpenIdConnectConstantsExt.Auth.IroncladAuthenticationScheme,
                OidcConstants.TokenTypes.RefreshToken);

            //TODO:@gafanasiev Get lifetime dynamically
            await _tokenService.SaveIroncladRefreshTokenAsync(sessionId, refreshToken);

            return new LykkeUserAuthenticationContext
            {
                UserId = lykkeUserId,
                Email = lykkeUser.Email,
                SessionId = sessionId
            };
        }

        public async Task SaveLykkeUserIdAfterExternalLoginAsync(ClaimsPrincipal principal)
        {
            var userId = principal.GetTokenClaim(ClaimTypes.NameIdentifier);

            var guid = await SaveLykkeUserIdForExternalLoginAsync(userId, _ironcladLykkeLoginsLifetime);

            // TODO:@gafanasiev check if this supports multiple instances.
            var protectedGuid = _dataProtector.Protect(guid);

            var useHttps = false;
#if !DEBUG
useHttps = true;
#endif
            _httpContextAccessor.HttpContext.Response.Cookies.Append(
                OpenIdConnectConstantsExt.Cookies.TemporaryUserIdCookie, protectedGuid, new CookieOptions
                {
                    HttpOnly = true,
                    MaxAge = _ironcladLykkeLoginsLifetime,
                    Secure = useHttps
                });
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
