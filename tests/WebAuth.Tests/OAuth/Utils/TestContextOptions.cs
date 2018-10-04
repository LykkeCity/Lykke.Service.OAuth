using System;
using System.Collections.Generic;
using System.Security.Claims;
using AspNet.Security.OpenIdConnect.Primitives;
using AspNet.Security.OpenIdConnect.Server;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using NSubstitute;

namespace WebAuth.Tests.OAuth.Utils
{
    /// <summary>
    ///     This is a comon class to mock any dependency for any Oauth handlers context.
    ///     If you need any dependency to call context constructor add it here and then fill in <see cref="Action" />.
    /// </summary>
    internal class TestContextOptions
    {
        private const string DefaultAuthSchemeName = "test_scheme";
        private const string DefaultAuthSchemeDisplayName = "test_scheme_display_name";

        [CanBeNull] internal string AuthSchemeName { get; set; }
        [CanBeNull] internal string AuthSchemeDisplayName { get; set; }
        [CanBeNull] internal IAuthenticationHandler AuthenticationHandler { get; set; }
        [CanBeNull] internal HttpContext HttpContext { get; set; }
        [CanBeNull] internal AuthenticationScheme AuthenticationScheme { get; set; }
        [CanBeNull] internal OpenIdConnectServerOptions OpenIdConnectServerOptions { get; set; }
        [CanBeNull] internal AuthenticationTicket AuthenticationTicket { get; set; }
        [CanBeNull] internal OpenIdConnectRequest OpenIdConnectRequest { get; set; }
        [CanBeNull] internal OpenIdConnectResponse OpenIdConnectResponse { get; set; }
        [CanBeNull] internal IEnumerable<Claim> Claims { get; set; } = null;

        internal void Initialize()
        {
            if (AuthSchemeName == null) AuthSchemeName = DefaultAuthSchemeName;

            if (AuthSchemeDisplayName == null) AuthSchemeDisplayName = DefaultAuthSchemeDisplayName;

            if (AuthenticationHandler == null) AuthenticationHandler = Substitute.For<IAuthenticationHandler>();

            if (HttpContext == null) HttpContext = Substitute.For<HttpContext>();

            if (AuthenticationScheme == null)
                AuthenticationScheme = new AuthenticationScheme(
                    AuthSchemeName, AuthSchemeDisplayName,
                    AuthenticationHandler?.GetType());

            if (OpenIdConnectServerOptions == null) OpenIdConnectServerOptions = new OpenIdConnectServerOptions();

            if (AuthenticationTicket == null)
            {
                var identity = Claims == null
                    ? new ClaimsIdentity()
                    : new ClaimsIdentity(Claims);

                var claimsPrincipal = new ClaimsPrincipal(identity);

                AuthenticationTicket = new AuthenticationTicket(claimsPrincipal, AuthSchemeName);
            }

            if (OpenIdConnectRequest == null) OpenIdConnectRequest = new OpenIdConnectRequest();

            if (OpenIdConnectResponse == null) OpenIdConnectResponse = new OpenIdConnectResponse();
        }
    }
}
