using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AspNet.Security.OpenIdConnect.Extensions;
using AspNet.Security.OpenIdConnect.Server;
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

        private static TestServer CreateAuthorizationServer(Action<OpenIdConnectServerOptions> configuration = null)
        {
            var builder = new WebHostBuilder();

            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                services.AddAuthentication();
                services.AddLogging();
            });

            builder.Configure(app =>
            {
                app.UseOpenIdConnectServer(options =>
                {
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

                    configuration?.Invoke(options);
                });

            });

            return new TestServer(builder);
        }
    }
}