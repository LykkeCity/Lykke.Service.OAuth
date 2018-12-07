using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using AspNet.Security.OpenIdConnect.Extensions;
using AspNet.Security.OpenIdConnect.Primitives;
using Common;
using Core.Extensions;
using Core.ExternalProvider;
using Core.ExternalProvider.Exceptions;
using Core.Services;
using IdentityModel;
using Lykke.Service.ClientAccount.Client;
using Lykke.Service.ClientAccount.Client.AutorestClient.Models;
using Lykke.Service.ClientAccount.Client.Models;
using Lykke.Service.PersonalData.Client.Models;
using Lykke.Service.PersonalData.Contract;
using Lykke.Service.PersonalData.Contract.Models;
using Lykke.Service.Session.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using StackExchange.Redis;

namespace Lykke.Service.OAuth.Services.ExternalProvider
{
    public class ExternalUserOperator : IExternalUserOperator
    {
        private const string RedisPrefixIroncladLykkeLogins = "OAuth:IroncladLykkeLogins";

        private readonly TimeSpan _ironcladLykkeLoginsLifetime = TimeSpan.FromMinutes(2);
        private readonly TimeSpan _mobileSessionLifetime = TimeSpan.FromDays(30);

        private readonly IDatabase _database;
        private readonly IIroncladUserRepository _ironcladUserRepository;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IClientAccountClient _clientAccountClient;
        private readonly IPersonalDataService _personalDataService;
        private readonly IDataProtector _dataProtector;
        private readonly IClientSessionsClient _clientSessionsClient;
        private readonly ITokenService _tokenService;
        private readonly IIroncladService _ironcladService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ExternalUserOperator(
            IIroncladUserRepository ironcladUserRepository,
            IHostingEnvironment hostingEnvironment,
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
            _ironcladUserRepository = ironcladUserRepository;
            _hostingEnvironment = hostingEnvironment;
            _clientAccountClient = clientAccountClient;
            _personalDataService = personalDataService;
            _httpContextAccessor = httpContextAccessor;
            _clientSessionsClient = clientSessionsClient;
            _tokenService = tokenService;
            _ironcladService = ironcladService;
            _dataProtector =
                dataProtectionProvider.CreateProtector(OpenIdConnectConstantsExt.Protectors
                    .ExternalProviderCookieProtector);
        }

        /// <inheritdoc />
        public Task<bool> AssociateIroncladUserAsync(string ironcladUserId, string lykkeUserId)
        {
            if (string.IsNullOrWhiteSpace(lykkeUserId))
                throw new ArgumentNullException(nameof(lykkeUserId));

            if (string.IsNullOrWhiteSpace(ironcladUserId))
                throw new ArgumentNullException(nameof(ironcladUserId));

            return _ironcladUserRepository.AddAsync(new IroncladUser
            {
                IroncladUserId = ironcladUserId,
                LykkeUserId = lykkeUserId
            });
        }

        /// <inheritdoc />
        public async Task<string> GetIroncladAssociatedLykkeUserIdAsync(string ironcladUserId)
        {
            if (string.IsNullOrWhiteSpace(ironcladUserId))
                throw new ArgumentNullException(nameof(ironcladUserId));

            var ironcladUser = await _ironcladUserRepository.GetByIdAsync(ironcladUserId);

            return ironcladUser.LykkeUserId;
        }

        /// <inheritdoc />
        public async Task<ClientAccountInformationModel> ProvisionIfNotExistAsync(ClaimsPrincipal principal)
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
                throw new AutoprovisionException(
                    $"Could not provision external user, idp:{idp}, externalUserId:{externalUserId}");

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

            await AssociateIroncladUserAsync(
                externalUserId,
                lykkeUserId);

            await _ironcladService.AddClaim(externalUserId, OpenIdConnectConstantsExt.Claims.Lsub, lykkeUserId);

            return newLykkeUser;
        }

        //ClaimNotFoundException
        /// <inheritdoc />
        public async Task<LykkeUserAuthenticationContext> AuthenticateAsync(ClaimsPrincipal principal)
        {
            var ironcladUserId = principal.GetTokenClaim(JwtClaimTypes.Subject);

            var identityProvider =
                principal.GetTokenClaim("http://schemas.microsoft.com/identity/claims/identityprovider");

            //Try to find id in lsub.
            var lsub = principal.FindFirst(OpenIdConnectConstantsExt.Claims.Lsub)?.Value;

            ClientAccountInformationModel lykkeUser = null;

            string lykkeUserId = null;

            var associatedUserId = await GetIroncladAssociatedLykkeUserIdAsync(ironcladUserId);

            var userAssociated = !string.IsNullOrWhiteSpace(associatedUserId);

            var ironcladUserHasLsubClaim = !string.IsNullOrWhiteSpace(lsub);

            var isNewUser = false;

            var userIdsAreEqual = string.Equals(lsub, associatedUserId);

            if (ironcladUserHasLsubClaim && userAssociated && !userIdsAreEqual)
                throw new AuthenticationException("User Id's are not synced.");

            if (ironcladUserHasLsubClaim)
            {
                lykkeUserId = lsub;
            }
            else
            {
                if (userAssociated)
                {
                    lykkeUserId = associatedUserId;
                }
                // If user is not associated and does not have lsub.
                else
                {
                    // If authenticated through lykke.
                    if (identityProvider.Equals(OpenIdConnectConstantsExt.Providers.Lykke))
                    {
                        // User id should be stored inside a cookie.
                        lykkeUserId = await GetLykkeUserIdFromCookieAsync();
                    }
                    else
                    {
                        // If authenticated through external identity provider.
                        // Should autoprovision user.
                        lykkeUser = await ProvisionIfNotExistAsync(principal);
                        isNewUser = true;
                    }
                }
            }

            if (!isNewUser) lykkeUser = await _clientAccountClient.GetClientByIdAsync(lykkeUserId);

            // We must be sure that user exists.
            if (lykkeUser == null)
                throw new AuthenticationException($"Lykke user does not exist. lykkeUserId: {lykkeUserId}");

            // If user is new, these steps are inside ProvisionIfNotExistAsync.
            if (!isNewUser)
            {
                if (!ironcladUserHasLsubClaim)
                    await _ironcladService.AddClaim(ironcladUserId, OpenIdConnectConstantsExt.Claims.Lsub, lykkeUserId);

                if (!userAssociated)
                    await AssociateIroncladUserAsync(
                        ironcladUserId,
                        lykkeUserId);
            }

            //TODO:@gafanasiev Think how to get already created session and use it.
            var clientSession =
                await _clientSessionsClient.Authenticate(lykkeUserId, string.Empty, null, null,
                    _mobileSessionLifetime);

            if (clientSession == null)
                throw new AuthenticationException($"Unable to create user session! lykkeUserId: {lykkeUserId}");

            var sessionId = clientSession.SessionToken;

            var refreshToken = await _httpContextAccessor.HttpContext.GetTokenAsync(
                OpenIdConnectConstantsExt.Auth.IroncladAuthenticationScheme,
                OidcConstants.TokenTypes.RefreshToken);

            await _tokenService.SaveIroncladRefreshTokenAsync(sessionId, refreshToken);

            return new LykkeUserAuthenticationContext
            {
                UserId = lykkeUser.Id,
                Email = lykkeUser.Email,
                SessionId = sessionId
            };
        }

        /// <inheritdoc />
        public async Task SaveLykkeUserIdAfterIroncladlLoginAsync(string lykkeUserId)
        {
            var guid = await SaveLykkeUserIdForExternalLoginAsync(lykkeUserId, _ironcladLykkeLoginsLifetime);

            // TODO:@gafanasiev check if this supports multiple instances.
            var protectedGuid = _dataProtector.Protect(guid);

            var useHttps = !_hostingEnvironment.IsDevelopment();

            _httpContextAccessor.HttpContext.Response.Cookies.Append(
                OpenIdConnectConstantsExt.Cookies.TemporaryUserIdCookie, protectedGuid, new CookieOptions
                {
                    HttpOnly = true,
                    Expires = DateTimeOffset.UtcNow.Add(_ironcladLykkeLoginsLifetime),
                    MaxAge = _ironcladLykkeLoginsLifetime,
                    Secure = useHttps
                });
        }

        /// <inheritdoc />
        public async Task<ClaimsPrincipal> GetCurrentUserAsync()
        {
            // Read external identity from the temporary cookie.
            var authenticateResult =
                await _httpContextAccessor.HttpContext.AuthenticateAsync(OpenIdConnectConstantsExt.Auth
                    .ExternalAuthenticationScheme);

            if (authenticateResult == null)
                throw new AuthenticationException("No authentication result");

            if (!authenticateResult.Succeeded)
                throw new AuthenticationException("Authentication failed", authenticateResult.Failure);

            return authenticateResult.Principal;
        }

        /// <inheritdoc />
        public async Task SignInAsync(LykkeUserAuthenticationContext context)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, context.UserId),
                new Claim(JwtClaimTypes.Email, context.Email),
                new Claim(JwtClaimTypes.Subject, context.UserId)
            };

            var identity = new ClaimsIdentity(new GenericIdentity(context.Email, "Token"), claims);

            // Add sessionId only to access token.
            identity.AddClaim(OpenIdConnectConstantsExt.Claims.SessionId, context.SessionId,
                OpenIdConnectConstants.Destinations.AccessToken);

            // delete temporary cookie used during external authentication
            await _httpContextAccessor.HttpContext.SignOutAsync(OpenIdConnectConstantsExt.Auth
                .ExternalAuthenticationScheme);

            // TODO: Think if we need to remove this step and authenticate directly with ASOS to issue tokens.
            await _httpContextAccessor.HttpContext.SignInAsync(OpenIdConnectConstantsExt.Auth.DefaultScheme,
                new ClaimsPrincipal(identity));
        }

        private string GetIroncladLykkeLoginsRedisKey(string guid)
        {
            if (string.IsNullOrWhiteSpace(guid))
                throw new ArgumentNullException(nameof(guid));

            return $"{RedisPrefixIroncladLykkeLogins}:{guid}";
        }

        private async Task<string> GetLykkeUserIdForExternalLoginAsync(string guid)
        {
            if (string.IsNullOrWhiteSpace(guid))
                throw new ArgumentNullException(nameof(guid));

            var redisKey = GetIroncladLykkeLoginsRedisKey(guid);

            var lykkeUserId = await _database.StringGetAsync(redisKey);

            if (lykkeUserId.HasValue)
                return lykkeUserId;

            return string.Empty;
        }

        private async Task<string> GetLykkeUserIdFromCookieAsync()
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
                throw new AuthenticationException("Authenticated through Lykke, but guid is not saved to cookie.");

            // TODO:@gafanasiev check if this supports multiple instances.
            var guid = _dataProtector.Unprotect(protectedGuid);

            var lykkeUserId = await GetLykkeUserIdForExternalLoginAsync(guid);

            if (string.IsNullOrWhiteSpace(lykkeUserId))
                throw new AuthenticationException(
                    $"Authenticated through Lykke, but lykkeUserId was not found for guid:{guid}.");

            return lykkeUserId;
        }

        private async Task<string> SaveLykkeUserIdForExternalLoginAsync(string lykkeUserId, TimeSpan ttl)
        {
            if (string.IsNullOrWhiteSpace(lykkeUserId))
                throw new ArgumentNullException(nameof(lykkeUserId));

            var guid = StringUtils.GenerateId();

            var redisKey = GetIroncladLykkeLoginsRedisKey(guid);
            await _database.StringSetAsync(redisKey, lykkeUserId, ttl);
            return guid;
        }
    }
}
