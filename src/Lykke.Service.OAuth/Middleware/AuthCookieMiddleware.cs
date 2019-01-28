using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Common.Log;
using Core.ExternalProvider.Settings;
using Lykke.Common.Log;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Lykke.Service.OAuth.Middleware
{
    public class AuthCookieMiddleware
    {
        private static readonly Regex Regex = new Regex(@"OS ((\d+_?){2,3})\s", RegexOptions.Compiled);
        private readonly RequestDelegate _next;
        private readonly ExternalProvidersSettings _externalProvidersSettings;
        private readonly ILog _log;

        public AuthCookieMiddleware(
            RequestDelegate next,
            ExternalProvidersSettings externalProvidersSettings,
            ILogFactory logFactory)
        {
            _next = next;
            _externalProvidersSettings = externalProvidersSettings;
            _log = logFactory.CreateLog(this);
        }

        public async Task Invoke(HttpContext context)
        {
            var iosVersion = GetIosVersion(context);

            if (!RequiresSameSiteCookieFix(iosVersion))
            {
                await _next.Invoke(context);
                return;
            }

            var schemeProvider = context.RequestServices.GetRequiredService<IAuthenticationSchemeProvider>();
            var handlerProvider = context.RequestServices.GetRequiredService<IAuthenticationHandlerProvider>();

            foreach (var scheme in await schemeProvider.GetRequestHandlerSchemesAsync())
                if (await handlerProvider.GetHandlerAsync(context,
                        scheme.Name) is IAuthenticationRequestHandler handler &&
                    await handler.HandleRequestAsync())
                {
                    string location = null;
                    if (context.Response.StatusCode == (int) HttpStatusCode.Redirect)
                        location = context.Response.Headers["location"];
                    else if (context.Request.Method == "GET" && !context.Request.Query["skip"].Any())
                        location = context.Request.Path + context.Request.QueryString + "&skip=1";

                    if (location != null)
                    {
                        _log.Info($"Replacing redirect with html page. ios version: {iosVersion}");

                        context.Response.ContentType = "text/html";
                        context.Response.StatusCode = (int) HttpStatusCode.OK;

                        var html = $@"
                                <html><head>
                                    <meta http-equiv='refresh' content='0;url={location}' />
                                </head></html>";
                        await context.Response.WriteAsync(html);
                    }

                    return;
                }

            await _next.Invoke(context);
        }

        // LINK: https://github.com/IdentityServer/IdentityServer4/issues/2595#issuecomment-425068595
        private bool RequiresSameSiteCookieFix(string version)
        {
            if (string.IsNullOrWhiteSpace(version))
                return false;

            var majorVersion = version
                .Replace("OS ", string.Empty, StringComparison.InvariantCultureIgnoreCase)
                .Split('_')[0];

            return int.TryParse(majorVersion, NumberStyles.Any, CultureInfo.InvariantCulture, out var v)
                   && v >= _externalProvidersSettings.RedirectSettings.IosMinVersionForCustomRedirect;
        }

        private string GetIosVersion(HttpContext context)
        {
            var userAgent = context.Request.Headers["User-Agent"].ToString();
            var groups = Regex.Matches(userAgent);

            if (groups.Count == 0) return string.Empty;

            var captures = groups[0].Captures;

            if (captures.Count == 0) return string.Empty;

            // Captured version might be in a form of a semver, ie. 'OS 10_3_0'

            return captures[0].Value;
        }
    }
}
