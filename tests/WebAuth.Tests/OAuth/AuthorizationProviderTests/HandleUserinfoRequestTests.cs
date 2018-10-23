using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AspNet.Security.OpenIdConnect.Primitives;
using FluentAssertions;
using FluentAssertions.Execution;
using Lykke.Service.ClientAccount.Client;
using Lykke.Service.ClientAccount.Client.Models;
using NSubstitute;
using WebAuth.Providers;
using WebAuth.Tests.OAuth.Utils;
using Xunit;

namespace WebAuth.Tests.OAuth.AuthorizationProviderTests
{
    public class HandleUserinfoRequestTests
    {
        private readonly IClientAccountClient _clientAccountClient;
        private readonly AuthorizationProvider _authorizationProvider;

        public HandleUserinfoRequestTests()
        {
            _clientAccountClient = Substitute.For<IClientAccountClient>();

            _authorizationProvider = AuthorizationProviderUtils.CreateAuthorizationProvider(options =>
            {
                options.ClientAccountClient = _clientAccountClient;
            });
        }

        [Fact]
        public async Task HandleUserinfoRequestTests_WhenSubIsSpecified_ClientAccountIsCalled()
        {
            // Arrange
            var clientId = Guid.NewGuid().ToString();
            _clientAccountClient.GetByIdAsync(clientId).Returns(new ClientModel());

            var context = AuthorizationProviderUtils.CreateHandleUserinfoRequestContext(
                options =>
                {
                    options.OpenIdConnectRequest = new OpenIdConnectRequest();
                });
            context.Subject = clientId;

            // Act
            await _authorizationProvider.HandleUserinfoRequest(context);

            // Assert
            using (new AssertionScope())
            {
                await _clientAccountClient.Received(1).GetByIdAsync(clientId);
            }
        }

        [Fact]
        public async Task HandleUserinfoRequestTests_WhenSubIsSpecified_PhoneIsSetUp()
        {
            // Arrange
            var clientId = Guid.NewGuid().ToString();
            var phone = "12412413251";
            _clientAccountClient.GetByIdAsync(clientId).Returns(new ClientModel(){Phone = phone});

            var context = AuthorizationProviderUtils.CreateHandleUserinfoRequestContext(
                options =>
                {
                    options.OpenIdConnectRequest = new OpenIdConnectRequest();
                });
            context.Subject = clientId;

            // Act
            await _authorizationProvider.HandleUserinfoRequest(context);

            // Assert
            using (new AssertionScope())
            {
                context.PhoneNumber.Should().BeEquivalentTo(phone);
                context.PhoneNumberVerified.Should().BeTrue();
            }
        }
    }
}
