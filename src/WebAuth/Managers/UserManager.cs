using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using AspNet.Security.OpenIdConnect.Extensions;
using AspNet.Security.OpenIdConnect.Server;
using AspNet.Security.OpenIdConnect.Primitives;
using Lykke.Service.PersonalData.Contract;
using Core.Extensions;
using Core.Kyc;

namespace WebAuth.Managers
{
    public class UserManager : IUserManager
    {
        private readonly IKycDocumentsRepository _kycDocumentsRepository;
        private readonly IPersonalDataService _personalDataService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserManager(IPersonalDataService personalDataService,
            IKycDocumentsRepository kycDocumentsRepository, IHttpContextAccessor httpContextAccessor)
        {
            _personalDataService = personalDataService;
            _kycDocumentsRepository = kycDocumentsRepository;
            _httpContextAccessor = httpContextAccessor;
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
                new Claim(OpenIdConnectConstants.Claims.Email, email)
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
            var documents = await _kycDocumentsRepository.GetAsync(clientId);

            var uploadedDocumentTypes = documents?.Select(d => d.Type);

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
