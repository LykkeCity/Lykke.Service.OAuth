using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AspNet.Security.OpenIdConnect.Primitives;
using AspNet.Security.OpenIdConnect.Server;
using Core.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Xunit;
using Microsoft.Extensions.DependencyInjection;

namespace WebAuth.Tests.OAuth
{
    public class OAuthRequestValidationTests
    {
        [Fact]
        public async Task EnsureThatOnlyPostLogoutRequestsAreValid()
        {
            // arrange
            var server = CreateAuthorizationServer();
            var client = server.CreateClient();

            var request = new HttpRequestMessage(HttpMethod.Get, "/connect/logout");

            // act
            var response = await client.SendAsync(request);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task EnsureThatOnlyPostAuthorizeRequestsAreValid()
        {
            // arrange
            var server = CreateAuthorizationServer();
            var client = server.CreateClient();

            var request = new HttpRequestMessage(HttpMethod.Get, "/connect/authorize");

            // act
            var response = await client.SendAsync(request);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        private static TestServer CreateAuthorizationServer()
        {
            var builder = new WebHostBuilder();

            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                services.AddAuthentication(options =>
                    {
                        options.DefaultScheme = OpenIdConnectConstantsExt.Auth.DefaultScheme;
                    })
                    .AddOpenIdConnectServer(options =>
                    {
                        options.AuthorizationEndpointPath = "/connect/authorize";
                        options.LogoutEndpointPath = "/connect/logout";
                        options.TokenEndpointPath = "/connect/token";
                        options.UserinfoEndpointPath = "/connect/userinfo";
                        
                        options.Provider = new OpenIdConnectServerProvider
                        {
                            OnValidateLogoutRequest = context =>
                            {
                                // Reject non-POST logout requests.
                                if (
                                    !string.Equals(context.HttpContext.Request.Method, "POST",
                                        StringComparison.OrdinalIgnoreCase))
                                {
                                    context.Reject(
                                        OpenIdConnectConstants.Errors.InvalidRequest,
                                        "Only POST requests are supported.");
                                }

                                return Task.FromResult(0);
                            },
                            OnValidateAuthorizationRequest = context =>
                            {
                                if (
                                    !string.Equals(context.HttpContext.Request.Method, "POST",
                                        StringComparison.OrdinalIgnoreCase))
                                {
                                    context.Reject(
                                        OpenIdConnectConstants.Errors.InvalidRequest,
                                        "Only POST requests are supported.");
                                }

                                return Task.FromResult(0);
                            }
                        };
                    });
                services.AddLogging();
            });

            builder.Configure(app =>
            {
                app.UseAuthentication();
            });

            return new TestServer(builder);
        }
    }
}
