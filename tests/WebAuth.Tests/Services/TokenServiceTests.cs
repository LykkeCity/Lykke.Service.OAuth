using System;
using System.Net.Http;
using System.Threading.Tasks;
using Core.ExternalProvider;
using Core.ExternalProvider.Settings;
using FluentAssertions;
using IdentityModel.Client;
using Lykke.Service.OAuth.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using NSubstitute;
using StackExchange.Redis;
using Xunit;

namespace WebAuth.Tests.Services
{
    public class TokenServiceTests
    {
        private readonly IDatabase _redisDatabase;
        private readonly TokenService _tokenService;

        private const string TestRefreshToken = "test_refresh_token";

        public TokenServiceTests()
        {
            _redisDatabase = Substitute.For<IDatabase>();

            var multiplexer = Substitute.For<IConnectionMultiplexer>();
            var clientFactory = Substitute.For<IHttpClientFactory>();
            var discoveryCache = Substitute.For<IDiscoveryCache>();
            var ironcladAuth = new IdentityProviderSettings();
            var clock = Substitute.For<ISystemClock>();
            var protectionProvider = Substitute.For<IDataProtectionProvider>();
            var tokensFactory = Substitute.For<IOpenIdTokensFactory>();
            multiplexer.GetDatabase().ReturnsForAnyArgs(_redisDatabase);

            _tokenService = new TokenService(
                ironcladAuth,
                multiplexer, 
                clientFactory, 
                discoveryCache,
                clock,
                protectionProvider,
                tokensFactory);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task RevokeRefreshTokenAsync_RefreshTokenIsEmpty_ReturnsFalse(string refreshToken)
        {
            // Act
            var result = await _tokenService.RevokeRefreshTokenAsync(refreshToken);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task RevokeRefreshTokenAsync_RefreshTokenDeletedFromWhitelist_ReturnsTrue()
        {
            // Arrange
            _redisDatabase.KeyDeleteAsync("").ReturnsForAnyArgs(true);

            // Act
            var result = await _tokenService.RevokeRefreshTokenAsync(TestRefreshToken);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task RevokeRefreshTokenAsync_RefreshTokenFailedToDeleteFromWhitelist_ReturnsFalse()
        {
            // Arrange
            _redisDatabase.KeyDeleteAsync("").ReturnsForAnyArgs(false);

            // Act
            var result = await _tokenService.RevokeRefreshTokenAsync(TestRefreshToken);

            // Assert
            result.Should().BeFalse();
        }


        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task IsRefreshTokenInWhitelistAsync_RefreshTokenIsEmpty_ReturnsFalse(string refreshToken)
        {
            // Act
            var result = await _tokenService.IsRefreshTokenInWhitelistAsync(refreshToken);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task IsRefreshTokenInWhitelistAsync_RefreshTokenExistInWhitelist_ReturnsTrue()
        {
            // Arrange
            _redisDatabase.KeyExistsAsync("").ReturnsForAnyArgs(true);

            // Act
            var result = await _tokenService.IsRefreshTokenInWhitelistAsync(TestRefreshToken);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task IsRefreshTokenInWhitelistAsync_RefreshTokenNotExistInWhitelist_ReturnsFalse()
        {
            // Arrange
            _redisDatabase.KeyExistsAsync("").ReturnsForAnyArgs(false);

            // Act
            var result = await _tokenService.IsRefreshTokenInWhitelistAsync(TestRefreshToken);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task UpdateRefreshTokenInWhitelistAsync_OldRefreshTokenAndNewRefreshTokenAreProvided_UpdateTokenInWhitelist()
        {
            // Arrange
            var oldRefreshToken = string.Concat("old_", TestRefreshToken);
            var newRefreshToken = string.Concat("new_", TestRefreshToken);

            _redisDatabase.KeyExistsAsync("").ReturnsForAnyArgs(false);

            // Act
            await _tokenService.UpdateRefreshTokenInWhitelistAsync(oldRefreshToken, newRefreshToken);

            // Assert
            _redisDatabase.ReceivedWithAnyArgs().CreateTransaction();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task UpdateRefreshTokenInWhitelistAsync_OldRefreshTokenNotProvidedAndNewRefreshTokenIsProvided_SaveNewTokenToWhitelist(string oldRefreshToken)
        {
            // Arrange
            var newRefreshToken = string.Concat("new_", TestRefreshToken);

            _redisDatabase.KeyExistsAsync("").ReturnsForAnyArgs(false);

            // Act
            await _tokenService.UpdateRefreshTokenInWhitelistAsync(oldRefreshToken, newRefreshToken);

            // Assert
            await _redisDatabase.ReceivedWithAnyArgs().StringSetAsync(newRefreshToken, true, Arg.Any<TimeSpan>());
        }
    }
}
