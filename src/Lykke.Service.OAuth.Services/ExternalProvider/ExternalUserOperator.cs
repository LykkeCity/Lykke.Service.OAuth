using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Common;
using Core.Extensions;
using Core.ExternalProvider;
using Core.ExternalProvider.Exceptions;
using Lykke.Service.ClientAccount.Client;
using Lykke.Service.ClientAccount.Client.AutorestClient.Models;
using Lykke.Service.PersonalData.Client.Models;
using Lykke.Service.PersonalData.Contract;
using Lykke.Service.PersonalData.Contract.Models;
using Lykke.Service.Session.Client;
using MessagePack;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using StackExchange.Redis;

namespace Lykke.Service.OAuth.Services.ExternalProvider
{
    public class ExternalUserOperator : IExternalUserOperator
    {
        private const string RedisPrefixIroncladLykkeLogins = "OAuth:IroncladLykkeLogins";

        private readonly TimeSpan _tempLykkeUserIdCookieLifetime = TimeSpan.FromMinutes(2);
        private readonly TimeSpan _lykkeSignInContextCookieLifetime = TimeSpan.FromMinutes(2);
        private readonly TimeSpan _ironcladRequestCookieLifetime = TimeSpan.FromMinutes(2);
        private readonly TimeSpan _mobileSessionLifetime = TimeSpan.FromDays(30);

        private readonly IDatabase _database;
        private readonly IIroncladUserRepository _ironcladUserRepository;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IClientAccountClient _clientAccountClient;
        private readonly IPersonalDataService _personalDataService;
        private readonly IDataProtector _dataProtector;
        private readonly IClientSessionsClient _clientSessionsClient;
        private readonly IIroncladFacade _ironcladFacade;
        private readonly IExternalProvidersValidation _validation;
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
            IIroncladFacade ironcladFacade,
            IExternalProvidersValidation validation)
        {
            _database = connectionMultiplexer.GetDatabase();
            _ironcladUserRepository = ironcladUserRepository;
            _hostingEnvironment = hostingEnvironment;
            _clientAccountClient = clientAccountClient;
            _personalDataService = personalDataService;
            _httpContextAccessor = httpContextAccessor;
            _clientSessionsClient = clientSessionsClient;
            _ironcladFacade = ironcladFacade;
            _validation = validation;
            _dataProtector =
                dataProtectionProvider.CreateProtector(OpenIdConnectConstantsExt.Protectors
                    .ExternalProviderCookieProtector);
        }

        /// <inheritdoc />
        public void SaveLykkeSignInContext(LykkeSignInContext context)
        {
            // TODO:@gafanasiev check if this supports multiple instances.
            var serializedData = MessagePackSerializer.Serialize(context);

            var protectedData = _dataProtector.Protect(serializedData);

            var bitString = Convert.ToBase64String(protectedData);

            var useHttps = !_hostingEnvironment.IsDevelopment();

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTimeOffset.UtcNow.Add(_lykkeSignInContextCookieLifetime),
                MaxAge = _lykkeSignInContextCookieLifetime,
                Secure = useHttps
            };

            _httpContextAccessor.HttpContext.Response.Cookies.Append(
                OpenIdConnectConstantsExt.Cookies.LykkeSignInContextCookie,
                bitString,
                cookieOptions);
        }

        /// <inheritdoc />
        public LykkeSignInContext GetLykkeSignInContext()
        {
            var cookieExist = _httpContextAccessor.HttpContext.Request.Cookies.TryGetValue(
                OpenIdConnectConstantsExt.Cookies.LykkeSignInContextCookie,
                out var protectedData);

            if (!cookieExist)
                return null;

            var bytes = Convert.FromBase64String(protectedData);

            var unprotectedData = _dataProtector.Unprotect(bytes);

            return MessagePackSerializer.Deserialize<LykkeSignInContext>(unprotectedData);
        }

        /// <inheritdoc />
        public void ClearLykkeSignInContext()
        {
            _httpContextAccessor.HttpContext.Response.Cookies.Delete(OpenIdConnectConstantsExt.Cookies
                .LykkeSignInContextCookie);
        }

        /// <inheritdoc />
        public async Task<LykkeUser> ProvisionIfNotExistAsync(IroncladUser ironcladUser,
            IEnumerable<Claim> originalClaims)
        {
            if (ironcladUser == null)
                throw new ArgumentNullException(nameof(ironcladUser));

            var existingLykkeUser = await _clientAccountClient.GetClientByExternalIdentityProvider(
                ironcladUser.Idp,
                ironcladUser.Id);

            if (existingLykkeUser != null) return new LykkeUser(ironcladUser);

            if (_validation.RequireEmailVerification && !ironcladUser.EmailVerified)
                throw new AutoprovisionException(
                    $"Email not verified, idp:{ironcladUser.Idp}, externalUserId:{ironcladUser.Id}");

            if (_validation.RequirePhoneVerification && !ironcladUser.PhoneVerified)
                throw new AutoprovisionException(
                    $"Phone not verified, idp:{ironcladUser.Idp}, externalUserId:{ironcladUser.Id}");

            var newLykkeUser =
                await _clientAccountClient.ProvisionAsync(new ExternalClientProvisionModel
                {
                    Email = ironcladUser.Email,
                    ExternalIdentityProviderId = ironcladUser.Idp,
                    ExternalUserId = ironcladUser.Id,
                    Phone = ironcladUser.Phone
                });

            if (newLykkeUser == null)
                throw new AutoprovisionException(
                    $"Could not provision external user, idp:{ironcladUser.Idp}, externalUserId:{ironcladUser.Id}");

            var claims = originalClaims.Select(claim => new IdentityProviderOriginalClaim
            {
                Name = claim.Type,
                Value = claim.Value
            });

            await _personalDataService.SaveClaimsAsync(new IdentityProviderClaimsModel
            {
                ExternalProviderId = ironcladUser.Idp,
                ExternalUserId = ironcladUser.Id,
                LykkeClientId = newLykkeUser.Id,
                EmailClaim = newLykkeUser.Email,
                PhoneClaim = newLykkeUser.Phone,
                OriginalClaims = claims
            });

            return new LykkeUser(ironcladUser);
        }

        /// <inheritdoc />
        public async Task<LykkeUserAuthenticationContext> CreateSessionAsync(LykkeUser lykkeUser)
        {
            if (lykkeUser == null)
                throw new AuthenticationException("No lykke user.");

            //FIXME:@gafanasiev Think how to get already created session and use it.
            var clientSession =
                await _clientSessionsClient.Authenticate(lykkeUser.Id, string.Empty, null, null,
                    _mobileSessionLifetime);

            if (clientSession == null)
                throw new AuthenticationException($"Unable to create user session! lykkeUserId: {lykkeUser.Id}");

            var sessionId = clientSession.SessionToken;

            return new LykkeUserAuthenticationContext
            {
                LykkeUser = lykkeUser,
                SessionId = sessionId
            };
        }

        /// <inheritdoc />
        public async Task SaveTempLykkeUserIdAsync(string lykkeUserId)
        {
            var guid = await SaveTempLykkeUserIdToDb(lykkeUserId, _tempLykkeUserIdCookieLifetime);

            // TODO:@gafanasiev check if this supports multiple instances.
            var protectedGuid = _dataProtector.Protect(guid);

            var useHttps = !_hostingEnvironment.IsDevelopment();

            _httpContextAccessor.HttpContext.Response.Cookies.Append(
                OpenIdConnectConstantsExt.Cookies.TemporaryUserIdCookie, protectedGuid, new CookieOptions
                {
                    HttpOnly = true,
                    Expires = DateTimeOffset.UtcNow.Add(_tempLykkeUserIdCookieLifetime),
                    MaxAge = _tempLykkeUserIdCookieLifetime,
                    Secure = useHttps
                });
        }

        /// <inheritdoc />
        public Task<string> GetTempLykkeUserIdAsync()
        {
            var guidExists = _httpContextAccessor.HttpContext.Request.Cookies.TryGetValue(
                OpenIdConnectConstantsExt.Cookies.TemporaryUserIdCookie,
                out var protectedGuid);

            if (!guidExists || string.IsNullOrWhiteSpace(protectedGuid))
                return Task.FromResult<string>(null);

            // TODO:@gafanasiev check if this supports multiple instances.
            var guid = _dataProtector.Unprotect(protectedGuid);

            return GetTempLykkeUserIdFromDb(guid);
        }

        /// <inheritdoc />
        public void ClearTempUserId()
        {
            _httpContextAccessor.HttpContext.Response.Cookies.Delete(OpenIdConnectConstantsExt.Cookies
                .TemporaryUserIdCookie);
        }

        /// <inheritdoc />
        public async Task AssociateIroncladUserAsync(LykkeUser lykkeUser, IroncladUser ironcladUser)
        {
            var ironcladUserId = ironcladUser.Id;

            var lykkeUserId = lykkeUser.Id;

            var lsub = ironcladUser.LykkeUserId;

            var associatedUser = await _ironcladUserRepository.GetByIdAsync(ironcladUserId);

            var userAssociated = associatedUser != null;

            var ironcladUserHasLsubClaim = !string.IsNullOrWhiteSpace(lsub);

            if (userAssociated && ironcladUserHasLsubClaim && !string.Equals(lsub, associatedUser.LykkeUserId))
                throw new AuthenticationException(
                    $"Ironclad user: {ironcladUserId} is associated with another lykke user:{associatedUser.LykkeUserId}.");

            if (!ironcladUserHasLsubClaim)
                await _ironcladFacade.AddUserClaimAsync(ironcladUserId, OpenIdConnectConstantsExt.Claims.Lsub,
                    lykkeUserId);

            if (!userAssociated)
                await _ironcladUserRepository.AddAsync(new IroncladUserBinding
                {
                    LykkeUserId = lykkeUserId,
                    IroncladUserId = ironcladUserId
                });
        }

        /// <inheritdoc />
        public async Task<string> AuthenticateLykkeUserAsync(string username, string password, string partnerId)
        {
            var lykkeUser = await _clientAccountClient.AuthenticateAsync(username, password, partnerId);

            return lykkeUser.Id;
        }

        /// <inheritdoc />
        public void SaveIroncladRequest(string redirectUrl)
        {
            _httpContextAccessor.HttpContext.Response.Cookies.Append(
                OpenIdConnectConstantsExt.Cookies.IroncladRequestCookie, redirectUrl, new CookieOptions
                {
                    HttpOnly = true,
                    Expires = DateTimeOffset.UtcNow.Add(_ironcladRequestCookieLifetime),
                    MaxAge = _ironcladRequestCookieLifetime
                });
        }

        /// <inheritdoc />
        public void ClearIroncladRequest()
        {
            _httpContextAccessor.HttpContext.Response.Cookies.Delete(OpenIdConnectConstantsExt.Cookies
                .IroncladRequestCookie);
        }

        private string GetIroncladLykkeLoginsRedisKey(string guid)
        {
            if (string.IsNullOrWhiteSpace(guid))
                throw new ArgumentNullException(nameof(guid));

            return $"{RedisPrefixIroncladLykkeLogins}:{guid}";
        }

        private async Task<string> GetTempLykkeUserIdFromDb(string guid)
        {
            if (string.IsNullOrWhiteSpace(guid))
                throw new ArgumentNullException(nameof(guid));

            var redisKey = GetIroncladLykkeLoginsRedisKey(guid);

            var lykkeUserId = await _database.StringGetAsync(redisKey);

            if (lykkeUserId.HasValue) return lykkeUserId;

            return string.Empty;
        }

        private async Task<string> SaveTempLykkeUserIdToDb(string lykkeUserId, TimeSpan ttl)
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
