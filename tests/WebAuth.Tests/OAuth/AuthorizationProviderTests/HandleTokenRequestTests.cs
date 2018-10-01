using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AspNet.Security.OpenIdConnect.Primitives;
using Core.Extensions;
using Core.Services;
using FluentAssertions;
using FluentAssertions.Execution;
using NSubstitute;
using WebAuth.Providers;
using WebAuth.Tests.OAuth.Utils;
using Xunit;

namespace WebAuth.Tests.OAuth.AuthorizationProviderTests
{
    public class HandleTokenRequestTests
    {
        private readonly IValidationService _validationService;
        private readonly AuthorizationProvider _authorizationProvider;

        public HandleTokenRequestTests()
        {
            _validationService = Substitute.For<IValidationService>();

            _authorizationProvider = AuthorizationProviderUtils.CreateAuthorizationProvider(options =>
            {
                options.ValidationService = _validationService;
            });
        }

        [Fact]
        public async Task HandleTokenRequest_WithoutSessionIdClaim_RejectRequest()
        {
            // Arrange
            var openIdRequest = new OpenIdConnectRequest
            {
                GrantType = OpenIdConnectConstants.GrantTypes.RefreshToken
            };

            var context = AuthorizationProviderUtils.CreateHandleTokenRequestContext(
                options => { options.OpenIdConnectRequest = openIdRequest; });

            // Act
            await _authorizationProvider.HandleTokenRequest(context);

            // Assert
            using (new AssertionScope())
            {
                context.IsRejected.Should().BeTrue();
                context.Error.Should().Be(OpenIdConnectConstantsExt.Errors.ClaimNotProvided);
                context.ErrorDescription.Should().Be("Session id is not provided in claims.");
            }
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task HandleTokenRequest_WithoutRefreshToken_RejectRequest(string refreshToken)
        {
            // Arrange
            var sessionIdClaim = new Claim(OpenIdConnectConstantsExt.Claims.SessionId, "test_session");

            var openIdRequest = new OpenIdConnectRequest
            {
                GrantType = OpenIdConnectConstants.GrantTypes.RefreshToken,
                // Assume refresh token was not passed.
                RefreshToken = refreshToken
            };

            var context = AuthorizationProviderUtils.CreateHandleTokenRequestContext(
                options =>
                {
                    options.OpenIdConnectRequest = openIdRequest;
                    options.Claims = new List<Claim> {sessionIdClaim};
                });

            // Act
            await _authorizationProvider.HandleTokenRequest(context);

            // Assert
            using (new AssertionScope())
            {
                context.IsRejected.Should().BeTrue();
                context.Error.Should().Be(OpenIdConnectConstants.Errors.InvalidRequest);
                context.ErrorDescription.Should().Be("refresh_token not present in request.");
            }
        }

        [Fact]
        public async Task HandleTokenRequest_RefreshTokenIsInvalid_RejectRequest()
        {
            // Arrange
            var sessionIdClaim = new Claim(OpenIdConnectConstantsExt.Claims.SessionId, "test_session");

            var openIdRequest = new OpenIdConnectRequest
            {
                GrantType = OpenIdConnectConstants.GrantTypes.RefreshToken,
                // Assume token was passed.
                RefreshToken = "test_token"
            };

            var context = AuthorizationProviderUtils.CreateHandleTokenRequestContext(
                options =>
                {
                    options.OpenIdConnectRequest = openIdRequest;
                    options.Claims = new List<Claim> {sessionIdClaim};
                });

            // Mock validation behavior, assume refresh token is INVALID.
            _validationService.IsRefreshTokenValidAsync("", "").ReturnsForAnyArgs(false);

            // Act
            await _authorizationProvider.HandleTokenRequest(context);

            // Assert
            using (new AssertionScope())
            {
                context.IsRejected.Should().BeTrue();
                context.Error.Should().Be(OpenIdConnectConstants.Errors.InvalidGrant);
                context.ErrorDescription.Should().Be("refresh_token was revoked.");
            }
        }

        [Fact]
        public async Task HandleTokenRequest_RefreshTokenIsValidAndSessionExists_AllowRequest()
        {
            // Arrange
            var sessionIdClaim = new Claim(OpenIdConnectConstantsExt.Claims.SessionId, "test_session");

            var openIdRequest = new OpenIdConnectRequest
            {
                GrantType = OpenIdConnectConstants.GrantTypes.RefreshToken,
                // Assume token was passed.
                RefreshToken = "test_token"
            };

            var context = AuthorizationProviderUtils.CreateHandleTokenRequestContext(
                options =>
                {
                    options.OpenIdConnectRequest = openIdRequest;
                    options.Claims = new List<Claim> {sessionIdClaim};
                });

            // Mock validation behavior, assume refresh token is VALID.
            _validationService.IsRefreshTokenValidAsync("", "").ReturnsForAnyArgs(true);

            // Act
            await _authorizationProvider.HandleTokenRequest(context);

            // Assert
            using (new AssertionScope())
            {
                context.IsRejected.Should().BeFalse();
                context.Error.Should().BeNullOrEmpty();
                context.ErrorDescription.Should().BeNullOrEmpty();
            }
        }

        [Fact]
        public async Task HandleTokenRequest_ContextContainsError_SkipToDefaultFlow()
        {
            // Arrange
            var openIdRequest = new OpenIdConnectRequest();

            var context = AuthorizationProviderUtils.CreateHandleTokenRequestContext(
                options => { options.OpenIdConnectRequest = openIdRequest; });

            const string error = "Test_Error";

            const string errorDescription = "Test_Error_Description";

            context.Reject(error, errorDescription);

            // Act
            await _authorizationProvider.HandleTokenRequest(context);

            // Assert
            using (new AssertionScope())
            {
                context.Error.Should().Be(error);
                context.ErrorDescription.Should().Be(errorDescription);
                await _validationService.DidNotReceiveWithAnyArgs().IsRefreshTokenValidAsync("", "");
            }
        }

        [Fact]
        public async Task HandleTokenRequest_NotRefreshTokenGrantType_SkipToDefaultFlow()
        {
            // Arrange
            var openIdRequest = new OpenIdConnectRequest
            {
                GrantType = OpenIdConnectConstants.GrantTypes.AuthorizationCode
            };

            var context = AuthorizationProviderUtils.CreateHandleTokenRequestContext(
                options => { options.OpenIdConnectRequest = openIdRequest; });

            // Act
            await _authorizationProvider.HandleTokenRequest(context);

            // Assert
            using (new AssertionScope())
            {
                context.Error.Should().BeNullOrEmpty();
                await _validationService.DidNotReceiveWithAnyArgs().IsRefreshTokenValidAsync("", "");
            }
        }
    }
}
