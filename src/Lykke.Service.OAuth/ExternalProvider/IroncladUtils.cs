using System.Linq;
using System.Threading.Tasks;
using AspNet.Security.OpenIdConnect.Primitives;
using Core.Extensions;
using Core.ExternalProvider;
using Core.Settings;
using IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Lykke.Service.OAuth.ExternalProvider
{
    public class IroncladUtils : IIroncladUtils
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IOpenIdTokensFactory _openIdTokensFactory;
        private readonly ISystemClock _clock;
        private readonly LifetimeSettings _lifetimeSettings;
        private readonly IDataProtector _dataProtector;
        private readonly IHostingEnvironment _hostingEnvironment;

        private const string IroncladLogoutSessionCookie = "IroncladLogoutSessionCookie";
        private const string IroncladLogoutSessionProtector = "IroncladLogoutSessionProtector";

        public IroncladUtils(
            IHttpContextAccessor httpContextAccessor,
            IOpenIdTokensFactory openIdTokensFactory,
            IDataProtectionProvider dataProtectionProvider,
            ISystemClock clock,
            LifetimeSettings lifetimeSettings,
            IHostingEnvironment hostingEnvironment)
        {
            _httpContextAccessor = httpContextAccessor;
            _openIdTokensFactory = openIdTokensFactory;
            _clock = clock;
            _lifetimeSettings = lifetimeSettings;
            _hostingEnvironment = hostingEnvironment;
            _dataProtector = dataProtectionProvider.CreateProtector(IroncladLogoutSessionProtector);
        }

        public async Task<OpenIdTokens> GetIroncladTokensAsync()
        {
            var authenticateResult =
                await _httpContextAccessor.HttpContext.AuthenticateAsync(
                    OpenIdConnectConstantsExt.Auth.IroncladAuthenticationScheme);

            var tokens = authenticateResult.Properties.GetTokens();

            string idToken = null;
            string accessToken = null;
            string refreshToken = null;

            var authenticationTokens = tokens as AuthenticationToken[] ?? tokens.ToArray();

            foreach (var token in authenticationTokens)
            {
                var value = token.Value;

                switch (token.Name)
                {
                    case OidcConstants.TokenTypes.IdentityToken:
                        idToken = value;
                        break;

                    case OidcConstants.TokenTypes.AccessToken:
                        accessToken = value;
                        break;

                    case OidcConstants.TokenTypes.RefreshToken:
                        refreshToken = value;
                        break;
                }
            }

            return _openIdTokensFactory.CreateOpenIdTokens(
                idToken,
                accessToken,
                refreshToken);
        }

        public void SaveIroncladLogoutContext(OpenIdConnectRequest request)
        {
            var useHttps = !_hostingEnvironment.IsDevelopment();

            var cookieLifetime = _lifetimeSettings.IroncladLogoutSessionLifetime;

            _httpContextAccessor.HttpContext.Response.Cookies.Append(IroncladLogoutSessionCookie,
                SerializeAndProtect(request), new CookieOptions
                {
                    IsEssential = true,
                    HttpOnly = true,
                    Secure = useHttps,
                    Expires = _clock.UtcNow.Add(cookieLifetime),
                    MaxAge = cookieLifetime
                });
        }

        public OpenIdConnectRequest GetIroncladLogoutContext()
        {
            var exists = _httpContextAccessor.HttpContext.Request.Cookies.TryGetValue(IroncladLogoutSessionCookie,
                out var serialized);

            return exists ? DeserializeAndUnprotect<OpenIdConnectRequest>(serialized) : null;
        }

        public void ClearIroncladLogoutContext()
        {
            _httpContextAccessor.HttpContext.Response.Cookies.Delete(IroncladLogoutSessionCookie);
        }

        //TODO:@gafanasiev Code duplication move to cookieManager.
        private string SerializeAndProtect<T>(T value)
        {
            var serialized = JsonConvert.SerializeObject(value);

            return _dataProtector.Protect(serialized);
        }

        //TODO:@gafanasiev Code duplication move to cookieManager.
        private T DeserializeAndUnprotect<T>(string value)
        {
            var unprotected = _dataProtector.Unprotect(value);

            return JsonConvert.DeserializeObject<T>(unprotected);
        }
    }
}
