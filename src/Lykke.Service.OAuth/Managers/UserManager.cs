using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using AspNet.Security.OpenIdConnect.Extensions;
using AspNet.Security.OpenIdConnect.Primitives;
using AspNet.Security.OpenIdConnect.Server;
using Core.Extensions;
using Core.ExternalProvider;
using IdentityModel;
using Lykke.Service.ClientAccount.Client;
using Lykke.Service.PersonalData.Contract;
using Microsoft.AspNetCore.Http;
using WebAuth.Managers;

namespace Lykke.Service.OAuth.Managers
{
    public class UserManager : IUserManager
    {
        private readonly IPersonalDataService _personalDataService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IClientAccountClient _clientAccountClient;

        public UserManager(
            IPersonalDataService personalDataService,
            IHttpContextAccessor httpContextAccessor,
            IClientAccountClient clientAccountClient
            )
        {
            _clientAccountClient = clientAccountClient;
            _personalDataService = personalDataService;
            _httpContextAccessor = httpContextAccessor;
        }

        public ClaimsIdentity CreateIdentity(List<string> scopes, IEnumerable<Claim> claims)
        {
            var identity = new ClaimsIdentity(OpenIdConnectServerDefaults.AuthenticationScheme);

            foreach (var claim in claims)
            {
                switch (claim.Type)
                {
                    case ClaimTypes.NameIdentifier:
                    {
                        identity.AddClaim(claim);
                        identity.AddClaim(OpenIdConnectConstants.Claims.Subject, claim.Value);
                        break;
                    }
                    case ClaimTypes.Name:
                    {
                        AddClaim(claim, identity);
                        break;
                    }
                    case OpenIdConnectConstants.Claims.Email:
                    {
                        if (scopes.Contains(OpenIdConnectConstants.Scopes.Email))
                            AddClaim(claim, identity);
                        break;
                    }
                    case OpenIdConnectConstants.Claims.EmailVerified:
                    {
                        if (scopes.Contains(OpenIdConnectConstants.Scopes.Email))
                            AddClaim(claim, identity);
                        break;
                    }
                    case OpenIdConnectConstants.Claims.PhoneNumber:
                    {
                        if (scopes.Contains(OpenIdConnectConstants.Scopes.Phone))
                            AddClaim(claim, identity);
                        break;
                    }
                    case OpenIdConnectConstants.Claims.PhoneNumberVerified:
                    {
                        if (scopes.Contains(OpenIdConnectConstants.Scopes.Phone))
                            AddClaim(claim, identity);
                        break;
                    }
                    case OpenIdConnectConstants.Claims.GivenName:
                    {
                        if (scopes.Contains(OpenIdConnectConstants.Scopes.Profile))
                            AddClaim(claim, identity);
                        break;
                    }
                    case OpenIdConnectConstants.Claims.FamilyName:
                    {
                        if (scopes.Contains(OpenIdConnectConstants.Scopes.Profile))
                            AddClaim(claim, identity);
                        break;
                    }
                    case OpenIdConnectConstantsExt.Claims.Country:
                    {
                        if (scopes.Contains(OpenIdConnectConstants.Scopes.Address))
                            AddClaim(claim, identity);
                        break;
                    }
                    case OpenIdConnectConstantsExt.Claims.SignType:
                    {
                        AddClaim(claim, identity);
                        break;
                    }
                    case OpenIdConnectConstantsExt.Claims.SessionId:
                    case OpenIdConnectConstantsExt.Claims.PartnerId:
                    {
                        AddClaim(claim, identity);
                        break;
                    }
                }
            }

            return identity;
        }

        public async Task<ClaimsIdentity> CreateUserIdentityAsync(string clientId, string email, string userName, string partnerId, string sessionId, bool? register = null)
        {
            var lykkeUser = await GetLykkeUserAsync(clientId);

            if (!string.IsNullOrWhiteSpace(email))
            {
                //TODO: Remove email from arguments.
                lykkeUser.Email = email;
            }

            if (!string.IsNullOrWhiteSpace(partnerId))
            {
                lykkeUser.PartnerId = partnerId;
            }
            
            var claims = ClaimsFromLykkeUser(lykkeUser);

            var identity = new ClaimsIdentity(new GenericIdentity(userName, "Token"), claims); 

            if (!string.IsNullOrWhiteSpace(sessionId))
                identity.AddClaim(new Claim(OpenIdConnectConstantsExt.Claims.SessionId, sessionId));

            if (register.HasValue)
                identity.AddClaim(new Claim(OpenIdConnectConstantsExt.Claims.SignType, register.Value ? "Register" : "Login"));

            return identity;
        }

        public ClaimsIdentity CreateUserIdentity(LykkeUserAuthenticationContext context)
        {
            if(string.IsNullOrWhiteSpace(context.SessionId))
                throw new ArgumentException("Session id must be specified!");

            var claims = ClaimsFromLykkeUser(context.LykkeUser);

            var identity = new ClaimsIdentity(claims, "Token"); 

            identity.AddClaim(new Claim(OpenIdConnectConstantsExt.Claims.SessionId, context.SessionId));

            return identity;
        }

        public IEnumerable<Claim> ClaimsFromLykkeUser(LykkeUser lykkeUser)
        {  
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, lykkeUser.Id),
                new Claim(OpenIdConnectConstants.Claims.Subject, lykkeUser.Id)
            };

            if (!string.IsNullOrEmpty(lykkeUser.Email))
            {
                claims.Add(new Claim(OpenIdConnectConstants.Claims.Email, lykkeUser.Email));
                claims.Add(new Claim(OpenIdConnectConstants.Claims.EmailVerified,
                    Convert.ToString(lykkeUser.EmailVerified)));
            }

            if (!string.IsNullOrEmpty(lykkeUser.Phone))
            {
                claims.Add(new Claim(OpenIdConnectConstants.Claims.PhoneNumber, lykkeUser.Phone));

                claims.Add(new Claim(OpenIdConnectConstants.Claims.PhoneNumberVerified, Convert.ToString(lykkeUser.PhoneVerified)));
            }

            if (!string.IsNullOrEmpty(lykkeUser.FirstName))
                claims.Add(new Claim(OpenIdConnectConstants.Claims.GivenName, lykkeUser.FirstName));

            if (!string.IsNullOrEmpty(lykkeUser.LastName))
                claims.Add(new Claim(OpenIdConnectConstants.Claims.FamilyName, lykkeUser.LastName));

            if (!string.IsNullOrEmpty(lykkeUser.Country))
                claims.Add(new Claim(OpenIdConnectConstantsExt.Claims.Country, lykkeUser.Country));

            if (!string.IsNullOrEmpty(lykkeUser.PartnerId))
                claims.Add(new Claim(OpenIdConnectConstantsExt.Claims.PartnerId, lykkeUser.PartnerId));

            return claims;
        }
        
        public string GetCurrentUserId()
        {
            return _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
        }

        private static void AddClaim(Claim claim, ClaimsIdentity identity)
        {
            if (identity.Claims.All(item => item.Type != claim.Type))
            {
                identity.AddClaim(new Claim(claim.Type, claim.Value)
                    .SetDestinations(OpenIdConnectConstants.Destinations.AccessToken,
                        OpenIdConnectConstants.Destinations.IdentityToken));
            }
        }

        public IroncladUser IroncladUserFromIdentity(ClaimsIdentity identity)
        {
            if (identity == null)
                throw new ArgumentNullException(nameof(identity));

            var lsub = identity.GetClaim(OpenIdConnectConstantsExt.Claims.Lsub);

            var idp = identity.GetClaim(OpenIdConnectConstantsExt.Claims.MicrosoftIdentityProvider);

            var user = new IroncladUser(GetBaseUser(identity))
            {
                LykkeUserId = lsub,
                Idp = idp
            };
            
            return user;
        }
        
        private static BaseUser GetBaseUser(ClaimsIdentity identity)
        {
            var id = identity.GetClaim(JwtClaimTypes.Subject);

            if (string.IsNullOrWhiteSpace(id))
            {
                id = identity.GetClaim(ClaimTypes.NameIdentifier);
            }

            var email = identity.GetClaim(JwtClaimTypes.Email);

            var emailVerified = identity.GetClaim(JwtClaimTypes.EmailVerified);

            var phone = identity.GetClaim(JwtClaimTypes.PhoneNumber);

            var phoneVerified = identity.GetClaim(JwtClaimTypes.PhoneNumberVerified);

            return new IroncladUser
            {
                Id = id,
                Email = email,
                Phone = phone,
                PhoneVerified = Convert.ToBoolean(phoneVerified),
                EmailVerified = Convert.ToBoolean(emailVerified)
            };
        }

        public async Task<LykkeUser> GetLykkeUserAsync(string lykkeUserId)
        {
            if (string.IsNullOrWhiteSpace(lykkeUserId))
                throw new ArgumentNullException(nameof(lykkeUserId));

            var personalData = await _personalDataService.GetAsync(lykkeUserId);
            if (personalData == null)
            {
                throw new InvalidOperationException("Unable to find personal data for user " + lykkeUserId);
            }

            var clientAccount = await _clientAccountClient.GetByIdAsync(lykkeUserId);
            if (clientAccount == null)
            {
                throw new InvalidOperationException("Unable to find client account data for user " + lykkeUserId);
            }

            return new LykkeUser
            {
                Id = lykkeUserId,
                Email = clientAccount.Email,
                EmailVerified = clientAccount.IsEmailVerified,
                Phone = personalData.ContactPhone,
                PhoneVerified=  clientAccount.IsPhoneVerified,
                FirstName =  personalData.FirstName,
                LastName = personalData.LastName,
                Country = personalData.Country,
                PartnerId = clientAccount.PartnerId
            };
        }
    }
}
