using System.Threading.Tasks;
using AspNet.Security.OpenIdConnect.Primitives;
using Core.Services;
using FluentAssertions;
using FluentAssertions.Execution;
using NSubstitute;
using WebAuth.Providers;
using WebAuth.Tests.OAuth.Utils;
using Xunit;

namespace WebAuth.Tests.OAuth.AuthorizationProviderTests
{
    public class ApplyTokenResponseTests
    {
        private readonly ITokenService _tokenService;
        private readonly AuthorizationProvider _authorizationProvider;

        public ApplyTokenResponseTests()
        {
            _tokenService = Substitute.For<ITokenService>();

            _authorizationProvider = AuthorizationProviderUtils.CreateAuthorizationProvider(options =>
            {
                options.TokenService = _tokenService;
            });
        }

        [Fact]
        public async Task ApplyTokenResponse_ContextContainsError_SkipToDefaultFlow()
        {
            // Arrange
            var error = "Test_error";

            var openIdRequest = new OpenIdConnectRequest
            {
                GrantType = OpenIdConnectConstants.GrantTypes.RefreshToken
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
                GrantType =  OpenIdConnectConstants.GrantTypes.ClientCredentials
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
        [InlineData(OpenIdConnectConstants.GrantTypes.RefreshToken)]
        [InlineData(OpenIdConnectConstants.GrantTypes.AuthorizationCode)]
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
