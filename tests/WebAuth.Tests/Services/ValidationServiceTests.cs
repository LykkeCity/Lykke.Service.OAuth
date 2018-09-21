using System.Threading.Tasks;
using Common.Log;
using Core.Services;
using FluentAssertions;
using FluentAssertions.Execution;
using Lykke.Common.Log;
using Lykke.Service.OAuth.Services;
using Lykke.Service.Session.Client;
using NSubstitute;
using NSubstitute.Extensions;
using Xunit;

namespace WebAuth.Tests.Services
{
    public class ValidationServiceTests
    {
        private readonly ILogFactory _logFactory;
        private readonly IClientSessionsClient _clientSessionsClient;
        private readonly ITokenService _tokenService;
        private readonly ValidationService _validationService;

        private const string TestRefreshToken = "test_refresh_token";
        private const string TestSessionId = "test_session_id";

        public ValidationServiceTests()
        {
            _clientSessionsClient = Substitute.For<IClientSessionsClient>();
            _tokenService = Substitute.For<ITokenService>();
            _logFactory = Substitute.For<ILogFactory>();

            var log = Substitute.For<ILog>();
            log.ReturnsForAll("");
            _logFactory.CreateLog(Arg.Any<ValidationService>()).ReturnsForAnyArgs(log);

            _validationService = new ValidationService(
                _logFactory,
                _clientSessionsClient,
                _tokenService);
        }

        [Theory]
        [InlineData(TestRefreshToken, null)]
        [InlineData(null, TestSessionId)]
        [InlineData(null, null)]
        public async Task IsRefreshTokenValidAsync_RefreshTokenAndSessionIdIsNullOrWhitespace_ReturnsFalse(string refreshToken, string sessionId)
        {
            // Act
            var result = await _validationService.IsRefreshTokenValidAsync(refreshToken, sessionId);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task IsRefreshTokenValidAsync_RefreshTokenNotExist_ReturnsFalse()
        {
            // Arrange

            // Assume token not exists in whitelist.
            _tokenService.IsRefreshTokenInWhitelistAsync("").ReturnsForAnyArgs(false);

            // Act
            var result = await _validationService.IsRefreshTokenValidAsync(TestRefreshToken, TestSessionId);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task IsRefreshTokenValidAsync_RefreshTokenExists_ReturnsTrue()
        {
            // Arrange

            // Assume token exists in whitelist.
            _tokenService.IsRefreshTokenInWhitelistAsync("").ReturnsForAnyArgs(true);

            // Act
            var result = await _validationService.IsRefreshTokenValidAsync(TestRefreshToken, TestSessionId);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task IsRefreshTokenValidAsync_SessionNotExist_ReturnsFalse()
        {
            // Arrange

            // Assume token exists in whitelist.
            _tokenService.IsRefreshTokenInWhitelistAsync("").ReturnsForAnyArgs(true);

            // Assume session was revoked.
            _clientSessionsClient.GetAsync("").ReturnsForAnyArgs((ClientSession) null);

            // Act
            var result = await _validationService.IsRefreshTokenValidAsync(TestRefreshToken, TestSessionId);

            // Assert
            using (new AssertionScope())
            {
                // If session was revoked we should revoke refresh token too.
                await _tokenService.Received().RevokeRefreshTokenAsync(TestRefreshToken);
                result.Should().BeFalse();
            }
        }

        [Fact]
        public async Task IsRefreshTokenValidAsync_SessionExists_ReturnsTrue()
        {
            // Arrange

            // Assume token exists in whitelist.
            _tokenService.IsRefreshTokenInWhitelistAsync("").ReturnsForAnyArgs(true);

            // Assume session is active.
            _clientSessionsClient.GetAsync("").ReturnsForAnyArgs(new ClientSession());

            // Act
            var result = await _validationService.IsRefreshTokenValidAsync(TestRefreshToken, TestSessionId);

            // Assert
            result.Should().BeTrue();
        }
    }
}
