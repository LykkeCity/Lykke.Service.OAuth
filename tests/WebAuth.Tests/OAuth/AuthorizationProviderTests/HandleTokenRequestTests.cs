using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AspNet.Security.OpenIdConnect.Primitives;
using Core.Application;
using Core.Extensions;
using Core.Services;
using FluentAssertions;
using FluentAssertions.Execution;
using Lykke.Service.Session.Client;
using NSubstitute;
using WebAuth.Providers;
using Xunit;

namespace WebAuth.Tests.OAuth.AuthorizationProviderTests
{
    public class HandleTokenRequestTests
    {
        private readonly IApplicationRepository _applicationRepository;
        private readonly IClientSessionsClient _clientSessionsClient;
        private readonly ITokenService _tokenService;
        private readonly IValidationService _validationService;
        private readonly AuthorizationProvider _authorizationProvider;

        private const string RefreshTokenGrantType = "refresh_token";
        private const string AuthorizationCodeTokenGrantType = "authorization_code";

        public HandleTokenRequestTests()
        {
            _applicationRepository = Substitute.For<IApplicationRepository>();
            _clientSessionsClient = Substitute.For<IClientSessionsClient>();
            _tokenService = Substitute.For<ITokenService>();
            _validationService = Substitute.For<IValidationService>();

            _authorizationProvider = new AuthorizationProvider(
                _applicationRepository,
                _clientSessionsClient,
                _tokenService,
                _validationService);
        }

        [Fact]
        public async Task HandleTokenRequest_WithoutSessionIdClaim_RejectRequest()
        {
            // Arrange
            var openIdRequest = new OpenIdConnectRequest
            {
                GrantType = RefreshTokenGrantType
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
                GrantType = RefreshTokenGrantType,
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
                GrantType = RefreshTokenGrantType,
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
                context.Error.Should().Be(OpenIdConnectConstants.Errors.InvalidRequest);
                context.ErrorDescription.Should().Be("Invalid request: refresh token was revoked.");
            }
        }

        [Fact]
        public async Task HandleTokenRequest_RefreshTokenIsValidAndSessionExists_AllowRequest()
        {
            // Arrange
            var sessionIdClaim = new Claim(OpenIdConnectConstantsExt.Claims.SessionId, "test_session");

            var openIdRequest = new OpenIdConnectRequest
            {
                GrantType = RefreshTokenGrantType,
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
                GrantType = AuthorizationCodeTokenGrantType
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
