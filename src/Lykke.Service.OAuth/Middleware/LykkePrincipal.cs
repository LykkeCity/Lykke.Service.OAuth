using System.Security.Claims;
using System.Threading.Tasks;
using Core.Extensions;
using IdentityModel.AspNetCore.OAuth2Introspection;
using Lykke.Common.ApiLibrary.Authentication;
using Lykke.Common.Cache;
using Lykke.Service.Session.Client;
using Microsoft.AspNetCore.Http;

namespace Lykke.Service.OAuth.Middleware
{
    internal class LykkePrincipal : ILykkePrincipal
    {
        private readonly OnDemandDataCache<ClaimsPrincipal> _claimsCache = new OnDemandDataCache<ClaimsPrincipal>();
        private readonly IClientSessionsClient _clientSessionsClient;

        private readonly IHttpContextAccessor _httpContextAccessor;

        public LykkePrincipal(IHttpContextAccessor httpContextAccessor, IClientSessionsClient clientSessionsClient)
        {
            _httpContextAccessor = httpContextAccessor;
            _clientSessionsClient = clientSessionsClient;
        }

        public string GetToken()
        {
            var func = TokenRetrieval.FromAuthorizationHeader();
            return func(_httpContextAccessor.HttpContext.Request);
        }

        public async Task<ClaimsPrincipal> GetCurrent()
        {
            var token = GetToken();

            if (string.IsNullOrWhiteSpace(token))
                return null;

            var result = _claimsCache.Get(token);
            if (result != null)
                return result;

            var session = await _clientSessionsClient.GetAsync(token);
            if (session == null)
                return null;

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, session.ClientId),
                new Claim(OpenIdConnectConstantsExt.Claims.SessionId, token)
            };
            var identity = new ClaimsIdentity(claims, OAuth2IntrospectionDefaults.AuthenticationScheme);

            if (session.PartnerId != null)
                identity.AddClaim(new Claim(OpenIdConnectConstantsExt.Claims.PartnerId, session.PartnerId));
            if (session.Pinned) identity.AddClaim(new Claim("TokenType", "Pinned"));

            result = new ClaimsPrincipal(identity);

            _claimsCache.Set(token, result);
            return result;
        }

        public void InvalidateCache(string token)
        {
            _claimsCache.Remove(token);
        }
    }
}
