using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Core.Extensions;
using Core.ExternalProvider;
using Core.ExternalProvider.Exceptions;
using Lykke.Service.ClientAccount.Client;
using Lykke.Service.ClientAccount.Client.AutorestClient.Models;
using Lykke.Service.PersonalData.Client.Models;
using Lykke.Service.PersonalData.Contract;
using Lykke.Service.PersonalData.Contract.Models;
using Lykke.Service.Session.Client;
using Microsoft.AspNetCore.Http;

namespace Lykke.Service.OAuth.Services.ExternalProvider
{
    public class ExternalUserOperator : IExternalUserOperator
    {
        private readonly TimeSpan _mobileSessionLifetime = TimeSpan.FromDays(30);
        private static string TemporaryUserIdKey = "TemporaryUserIdKey";
        private static string IroncladRequestKey = "IroncladRequestKey";
        private static string LykkeSignInContextKey = "LykkeSignInContextKey";
        private static string EndUserSessionKey = "EndUserSessionKey";

        private readonly IIroncladUserRepository _ironcladUserRepository;
        private readonly IClientAccountClient _clientAccountClient;
        private readonly IPersonalDataService _personalDataService;
        private readonly IClientSessionsClient _clientSessionsClient;
        private readonly IIroncladFacade _ironcladFacade;
        private readonly IExternalProvidersValidation _validation;
        private readonly IUserSession _userSession;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ExternalUserOperator(
            IIroncladUserRepository ironcladUserRepository,
            IClientAccountClient clientAccountClient,
            IPersonalDataService personalDataService,
            IHttpContextAccessor httpContextAccessor,
            IClientSessionsClient clientSessionsClient,
            IIroncladFacade ironcladFacade,
            IExternalProvidersValidation validation,
            IUserSession userSession)
        {
            _ironcladUserRepository = ironcladUserRepository;
            _clientAccountClient = clientAccountClient;
            _personalDataService = personalDataService;
            _httpContextAccessor = httpContextAccessor;
            _clientSessionsClient = clientSessionsClient;
            _ironcladFacade = ironcladFacade;
            _validation = validation;
            _userSession = userSession;
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
        public async Task<LykkeUserAuthenticationContext> CreateLykkeSessionAsync(LykkeUser lykkeUser)
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

            return lykkeUser?.Id;
        }

        
        /// <inheritdoc />
        public Task SaveLykkeSignInContextAsync(string originalUrl)
        {
            return _userSession.SetAsync(LykkeSignInContextKey, originalUrl);
        }

        /// <inheritdoc />
        public Task<string> GetLykkeSignInContextAsync()
        {
            return _userSession.GetAsync<string>(LykkeSignInContextKey);
        }

        /// <inheritdoc />
        public Task ClearLykkeSignInContextAsync()
        {
            return _userSession.DeleteAsync(LykkeSignInContextKey);
        }

        /// <inheritdoc />
        public Task SaveTempLykkeUserIdAsync(string lykkeUserId)
        {
            return _userSession.SetAsync(TemporaryUserIdKey, lykkeUserId);
        }

        /// <inheritdoc />
        public Task<string> GetTempLykkeUserIdAsync()
        {
            return _userSession.GetAsync<string>(TemporaryUserIdKey);
        }

        /// <inheritdoc />
        public Task ClearTempLykkeUserIdAsync()
        {
            return _userSession.DeleteAsync(TemporaryUserIdKey);
        }

        /// <inheritdoc />
        public Task SaveIroncladRequestAsync(string redirectUrl)
        {
            return _userSession.SetAsync(IroncladRequestKey, redirectUrl);
        }

        /// <inheritdoc />
        public Task<string> GetIroncladRequestAsync()
        {
            return _userSession.GetAsync<string>(IroncladRequestKey);
        }

        /// <inheritdoc />
        public Task ClearIroncladRequestAsync()
        {
            return _userSession.DeleteAsync(IroncladRequestKey);
        }

        public async Task EndUserSessionAsync()
        {
            await _userSession.EndSessionAsync();
        }
    }
}
