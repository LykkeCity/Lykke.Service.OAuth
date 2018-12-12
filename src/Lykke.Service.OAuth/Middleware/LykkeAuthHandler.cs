using System.Text.Encodings.Web;
using System.Threading.Tasks;
using IdentityModel.AspNetCore.OAuth2Introspection;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lykke.Service.OAuth.Middleware
{
    internal class LykkeAuthHandler : AuthenticationHandler<LykkeAuthOptions>
    {
        private readonly ILykkePrincipal _lykkePrincipal;
        private const int LykkeTokenLength = 64;

        public LykkeAuthHandler(IOptionsMonitor<LykkeAuthOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock, ILykkePrincipal lykkePrincipal)
            : base(options, logger, encoder, clock)
        {
            _lykkePrincipal = lykkePrincipal;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var token = TokenRetrieval.FromAuthorizationHeader()(Context.Request);
            if (token == null)
            {
                return AuthenticateResult.NoResult();
            }

            if (token.Length != LykkeTokenLength)
            {
                return AuthenticateResult.Fail("");
            }

            var principal = await _lykkePrincipal.GetCurrent();

            if (principal == null)
                return AuthenticateResult.NoResult();

            var ticket = new AuthenticationTicket(principal, "LykkeScheme");

            return AuthenticateResult.Success(ticket);
        }
    }
}
