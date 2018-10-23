using System;
using AspNet.Security.OpenIdConnect.Server;
using WebAuth.Providers;

namespace WebAuth.Tests.OAuth.Utils
{
    /// <summary>
    ///     Utitlity class for testing <see cref="AuthorizationProvider" />.
    /// </summary>
    internal static class AuthorizationProviderUtils
    {
        /// <summary>
        /// Helper method for creating <see cref="AuthorizationProvider" /> with mocked dependencies.
        /// </summary>
        /// <param name="optionsConfig"></param>
        /// <returns></returns>
        internal static AuthorizationProvider CreateAuthorizationProvider(Action<TestAuthorizationProviderOptions> optionsConfig = null)
        {
            var providerOptions = new TestAuthorizationProviderOptions();

            optionsConfig?.Invoke(providerOptions);

            return new AuthorizationProvider(
                providerOptions.ApplicationRepository,
                providerOptions.ClientSessionsClient,
                providerOptions.ClientAccountClient,
                providerOptions.TokenService,
                providerOptions.ValidationService,
                providerOptions.LogFactory);
        }

        /// <summary>
        ///     Helper method for creating fake for <see cref="ApplyTokenResponseContext" />.
        /// </summary>
        /// <param name="contextAction">
        ///     Action for configuring necessary parameters on <see cref="TestContextOptions" />.
        /// </param>
        /// <returns>Fake for <see cref="ApplyTokenResponseContext" />.</returns>
        internal static ApplyTokenResponseContext CreateApplyTokenResponseContext(
            Action<TestContextOptions> contextAction = null)
        {
            var testContextOptions = new TestContextOptions();

            contextAction?.Invoke(testContextOptions);

            testContextOptions.Initialize();

            return new ApplyTokenResponseContext(
                testContextOptions.HttpContext,
                testContextOptions.AuthenticationScheme,
                testContextOptions.OpenIdConnectServerOptions,
                testContextOptions.AuthenticationTicket,
                testContextOptions.OpenIdConnectRequest,
                testContextOptions.OpenIdConnectResponse);
        }

        /// <summary>
        ///     Helper method for creating fake for <see cref="HandleUserinfoRequestContext" />.
        /// </summary>
        /// <param name="contextAction">
        ///     Action for configuring necessary parameters on <see cref="TestContextOptions" />.
        /// </param>
        /// <returns>Fake for <see cref="HandleUserinfoRequestContext" />.</returns>
        internal static HandleUserinfoRequestContext CreateHandleUserinfoRequestContext(
            Action<TestContextOptions> contextAction = null)
        {
            var testContextOptions = new TestContextOptions();

            contextAction?.Invoke(testContextOptions);

            testContextOptions.Initialize();

            return new HandleUserinfoRequestContext(
                testContextOptions.HttpContext,
                testContextOptions.AuthenticationScheme,
                testContextOptions.OpenIdConnectServerOptions,
                testContextOptions.OpenIdConnectRequest,
                testContextOptions.AuthenticationTicket);
        }

        /// <summary>
        ///     Helper method for creating fake for <see cref="HandleTokenRequestContext" />.
        /// </summary>
        /// <param name="contextAction">
        ///     Action for configuring necessary parameters on <see cref="TestContextOptions" />.
        /// </param>
        /// <returns>Fake for <see cref="HandleTokenRequestContext" />.</returns>
        internal static HandleTokenRequestContext CreateHandleTokenRequestContext(
            Action<TestContextOptions> contextAction = null)
        {
            var testContextOptions = new TestContextOptions();

            contextAction?.Invoke(testContextOptions);

            testContextOptions.Initialize();

            return new HandleTokenRequestContext(
                testContextOptions.HttpContext,
                testContextOptions.AuthenticationScheme,
                testContextOptions.OpenIdConnectServerOptions,
                testContextOptions.OpenIdConnectRequest,
                testContextOptions.AuthenticationTicket);
        }
    }
}
