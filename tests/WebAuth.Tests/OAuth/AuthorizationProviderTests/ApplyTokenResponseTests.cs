using System.Threading.Tasks;
using AspNet.Security.OpenIdConnect.Primitives;
using Core.Application;
using Core.Services;
using FluentAssertions;
using FluentAssertions.Execution;
using Lykke.Service.Session.Client;
using NSubstitute;
using WebAuth.Providers;
using Xunit;

namespace WebAuth.Tests.OAuth.AuthorizationProviderTests
{
    public class ApplyTokenResponseTests
    {
        private readonly IApplicationRepository _applicationRepository;
        private readonly IClientSessionsClient _clientSessionsClient;
        private readonly ITokenService _tokenService;
        private readonly IValidationService _validationService;
        private readonly AuthorizationProvider _authorizationProvider;

        private const string RefreshTokenGrantType = "refresh_token";
        private const string AuthorizationCodeTokenGrantType = "authorization_code";
        private const string ClientCredentialsTokenGrantType = "client_credentials";

        public ApplyTokenResponseTests()
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
        public async Task ApplyTokenResponse_ContextContainsError_SkipToDefaultFlow()
        {
            // Arrange
            var error = "Test_error";

            var openIdRequest = new OpenIdConnectRequest
            {
                GrantType = RefreshTokenGrantType
            };

            var openIdResponse = new OpenIdConnectResponse
            {
                Error = error
            };

            var context = AuthorizationProviderUtils.CreateApplyTokenResponseContext(
                options =>
                {
                    options.OpenIdConnectRequest = openIdRequest;
                    options.OpenIdConnectResponse = openIdResponse;
                });

            // Act
            await _authorizationProvider.ApplyTokenResponse(context);

            // Assert
            using (new AssertionScope())
            {
                context.Error.Should().Be(error);
                await _tokenService.DidNotReceiveWithAnyArgs().UpdateRefreshTokenInWhitelistAsync("", "");
            }
        }


        [Fact]
        public async Task ApplyTokenResponse_NotRefreshTokenGrantTypeOrAuthCodeGrantType_SkipToDefaultFlow()
        {
            // Arrange
            var openIdRequest = new OpenIdConnectRequest
            {
                GrantType = ClientCredentialsTokenGrantType
            };

            var context = AuthorizationProviderUtils.CreateApplyTokenResponseContext(
                options => { options.OpenIdConnectRequest = openIdRequest; });

            // Act
            await _authorizationProvider.ApplyTokenResponse(context);

            // Assert
            using (new AssertionScope())
            {
                context.Error.Should().BeNullOrEmpty();
                await _tokenService.DidNotReceiveWithAnyArgs().UpdateRefreshTokenInWhitelistAsync("", "");
            }
        }

        [Theory]
        [InlineData(RefreshTokenGrantType)]
        [InlineData(AuthorizationCodeTokenGrantType)]
        public async Task ApplyTokenResponse_GrantTypeSupportsRefreshToken_UpdatesRefreshToken(string grantType)
        {
            // Arrange
            var oldRefreshToken = "Test_Old_RefreshToken";
            var newRefreshToken = "Test_New_RefreshToken";

            var openIdRequest = new OpenIdConnectRequest
            {
                GrantType = grantType,
                RefreshToken = oldRefreshToken
            };

            var openIdResponse = new OpenIdConnectResponse
            {
                RefreshToken = newRefreshToken
            };

            var context = AuthorizationProviderUtils.CreateApplyTokenResponseContext(
                options =>
                {
                    options.OpenIdConnectRequest = openIdRequest;
                    options.OpenIdConnectResponse = openIdResponse;
                });

            // Act
            await _authorizationProvider.ApplyTokenResponse(context);

            // Assert
            using (new AssertionScope())
            {
                context.Error.Should().BeNullOrEmpty();
                await _tokenService.Received().UpdateRefreshTokenInWhitelistAsync(oldRefreshToken, newRefreshToken);
            }
        }
    }
}
