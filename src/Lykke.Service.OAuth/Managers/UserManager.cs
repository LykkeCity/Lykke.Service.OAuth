using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using AspNet.Security.OpenIdConnect.Extensions;
using AspNet.Security.OpenIdConnect.Primitives;
using AspNet.Security.OpenIdConnect.Server;
using Common.Log;
using Core.Extensions;
using Lykke.Service.PersonalData.Contract;
using Microsoft.AspNetCore.Http;

namespace WebAuth.Managers
{
    public class UserManager : IUserManager
    {
        private readonly IPersonalDataService _personalDataService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILog _log;

        public UserManager(IPersonalDataService personalDataService,
            IHttpContextAccessor httpContextAccessor,
            ILog log
            )
        {
            _personalDataService = personalDataService;
            _httpContextAccessor = httpContextAccessor;
            _log = log.CreateComponentScope(nameof(UserManager));
        }

        public ClaimsIdentity CreateIdentity(List<string> scopes, IEnumerable<Claim> claims)
        {
            var identity = new ClaimsIdentity(OpenIdConnectServerDefaults.AuthenticationScheme);

            foreach (var claim in claims)
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
                        {
                            AddClaim(claim, identity);
                            break;
                        }
                }

            return identity;
        }

        public async Task<ClaimsIdentity> CreateUserIdentityAsync(string clientId, string email, string userName, string sessionId, bool? register = null)
        {
            var personalData = await _personalDataService.GetAsync(clientId);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, clientId),
                new Claim(OpenIdConnectConstants.Claims.Email, email),
                new Claim(OpenIdConnectConstants.Claims.Subject, clientId),
                new Claim(OpenIdConnectConstantsExt.Claims.SessionId,sessionId)
            };

            if (!string.IsNullOrEmpty(personalData.FirstName))
                claims.Add(new Claim(OpenIdConnectConstants.Claims.GivenName, personalData.FirstName));

            if (!string.IsNullOrEmpty(personalData.LastName))
                claims.Add(new Claim(OpenIdConnectConstants.Claims.FamilyName, personalData.LastName));

            if (!string.IsNullOrEmpty(personalData.Country))
                claims.Add(new Claim(OpenIdConnectConstantsExt.Claims.Country, personalData.Country));

            if (register.HasValue)
                claims.Add(new Claim(OpenIdConnectConstantsExt.Claims.SignType, register.Value ? "Register" : "Login"));

            return new ClaimsIdentity(new GenericIdentity(userName, "Token"), claims);
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
    }
}
