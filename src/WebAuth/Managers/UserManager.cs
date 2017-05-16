using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using AspNet.Security.OpenIdConnect.Extensions;
using AspNet.Security.OpenIdConnect.Server;
using Common.OpenIdConnect;
using Core.Clients;
using Core.Kyc;
using Microsoft.AspNetCore.Http;

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
                }

            return identity;
        }

        public async Task<ClaimsIdentity> CreateUserIdentityAsync(IClientAccount clientAccount, string userName)
        {
            var personalData = await _personalDataService.GetAsync(clientAccount.Id);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, clientAccount.Email),
                new Claim(ClaimTypes.NameIdentifier, clientAccount.Id),
                new Claim(OpenIdConnectConstants.Claims.Email, clientAccount.Email)
            };

            if (!string.IsNullOrEmpty(personalData.FirstName))
                claims.Add(new Claim(OpenIdConnectConstants.Claims.GivenName, personalData.FirstName));

            if (!string.IsNullOrEmpty(personalData.LastName))
                claims.Add(new Claim(OpenIdConnectConstants.Claims.FamilyName, personalData.LastName));

            if (!string.IsNullOrEmpty(personalData.Country))
                claims.Add(new Claim(OpenIdConnectConstantsExt.Claims.Country, personalData.Country));

            var documents = (await GetDocumentListAsync(clientAccount.Id)).ToList();
            if (documents.Any())
                claims.Add(new Claim(OpenIdConnectConstantsExt.Claims.Documents, string.Join(",", documents)));

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
            var destinations = new[]
            {
                OpenIdConnectConstants.Destinations.AccessToken,
                OpenIdConnectConstants.Destinations.IdentityToken
            };
            claim.SetDestinations(destinations);

            identity.AddClaim(claim);
        }
    }
}