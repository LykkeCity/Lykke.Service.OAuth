using System.Security.Claims;
using System.Threading.Tasks;
using Core.Extensions;
using IdentityModel.AspNetCore.OAuth2Introspection;
using Lykke.Common.Extensions;
using Lykke.Service.Session.Client;
using Microsoft.AspNetCore.Http;

namespace Lykke.Service.OAuth.Middleware
{
    internal class LykkePrincipal : ILykkePrincipal
    {
        private readonly ClaimsCache _claimsCache = new ClaimsCache();
        private readonly IClientSessionsClient _clientSessionsClient;

        private readonly IHttpContextAccessor _httpContextAccessor;

        public LykkePrincipal(IHttpContextAccessor httpContextAccessor, IClientSessionsClient clientSessionsClient)
        {
            _httpContextAccessor = httpContextAccessor;
            _clientSessionsClient = clientSessionsClient;
        }

        public string GetToken()
        {
            var context = _httpContextAccessor.HttpContext;

            var header = context.GetHeaderValueAs<string>("Authorization");

            if (string.IsNullOrEmpty(header))
                return null;

            var values = header.Split(' ');

            if (values.Length != 2)
                return null;

            if (values[0] != OAuth2IntrospectionDefaults.AuthenticationScheme)
                return null;

            return values[1];
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
            {
                identity.AddClaim(new Claim(OpenIdConnectConstantsExt.Claims.PartnerId, session.PartnerId));
            }
            if (session.Pinned)
            {
                identity.AddClaim(new Claim("TokenType", "Pinned"));
            }

            result = new ClaimsPrincipal(identity);

            _claimsCache.Set(token, result);
            return result;
        }
    }
}
