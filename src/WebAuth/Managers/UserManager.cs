using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using AspNet.Security.OpenIdConnect.Extensions;
using AspNet.Security.OpenIdConnect.Primitives;
using AspNet.Security.OpenIdConnect.Server;
using Core.Extensions;
using Lykke.Service.Kyc.Abstractions.Domain.Profile;
using Lykke.Service.Kyc.Abstractions.Services;
using Lykke.Service.PersonalData.Contract;
using Microsoft.AspNetCore.Http;

namespace WebAuth.Managers
{
    public class UserManager : IUserManager
    {
        private readonly IPersonalDataService _personalDataService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IKycProfileServiceV2 _kycProfileService;

        public UserManager(IPersonalDataService personalDataService,
            IHttpContextAccessor httpContextAccessor,
            IKycProfileServiceV2 kycProfileService
            )
        {
            _personalDataService = personalDataService;
            _httpContextAccessor = httpContextAccessor;
            _kycProfileService = kycProfileService;
        }

        public ClaimsIdentity CreateIdentity(List<string> scopes, IEnumerable<Claim> claims)
        {
            var identity = new ClaimsIdentity(OpenIdConnectServerDefaults.AuthenticationScheme);

            foreach (var claim in claims)
                switch (claim.Type)
                {
                    case ClaimTypes.NameIdentifier:
                    {
                        AddClaim(claim, identity);
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
                    case OpenIdConnectConstantsExt.Claims.Documents:
                    {
                        if (scopes.Contains(OpenIdConnectConstants.Scopes.Profile))
                            AddClaim(claim, identity);
                        break;
                    }
                    case OpenIdConnectConstantsExt.Claims.SignType:
                    {
                        AddClaim(claim, identity);
                        break;
                    }
                }

            return identity;
        }

        public async Task<ClaimsIdentity> CreateUserIdentityAsync(string clientId, string email, string userName, bool? register = null)
        {
            var personalData = await _personalDataService.GetAsync(clientId);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, clientId),
                new Claim(OpenIdConnectConstants.Claims.Email, email),
                new Claim(OpenIdConnectConstants.Claims.Subject, clientId)
            };

            if (!string.IsNullOrEmpty(personalData.FirstName))
                claims.Add(new Claim(OpenIdConnectConstants.Claims.GivenName, personalData.FirstName));

            if (!string.IsNullOrEmpty(personalData.LastName))
                claims.Add(new Claim(OpenIdConnectConstants.Claims.FamilyName, personalData.LastName));

            if (!string.IsNullOrEmpty(personalData.Country))
                claims.Add(new Claim(OpenIdConnectConstantsExt.Claims.Country, personalData.Country));

            var documents = (await GetDocumentListAsync(clientId)).ToList();

            if (documents.Any())
                claims.Add(new Claim(OpenIdConnectConstantsExt.Claims.Documents, string.Join(",", documents)));

            if (register.HasValue)
                claims.Add(new Claim(OpenIdConnectConstantsExt.Claims.SignType, register.Value ? "Register": "Login"));

            return new ClaimsIdentity(new GenericIdentity(userName, "Token"), claims);
        }

        public string GetCurrentUserId()
        {
            return _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
        }

        private async Task<IEnumerable<string>> GetDocumentListAsync(string clientId)
        {
            var documents = await _kycProfileService.GetDocumentsAsync(clientId, KycProfile.Default);

            var uploadedDocumentTypes = documents?.Select(d => d.Value.Type.Name);

            return uploadedDocumentTypes;
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
