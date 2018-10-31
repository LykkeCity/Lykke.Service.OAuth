using System.Threading.Tasks;
using Core.PasswordValidation;
using FluentAssertions;
using Lykke.Service.OAuth.Services.PasswordValidation.Validators;
using NSubstitute;
using Xunit;

namespace WebAuth.Tests.PasswordValidation
{
    public class PwnedPasswordsValidatorTests
    {
        private readonly IPasswordValidator _pwnedPasswordsValidator;
        private readonly IPwnedPasswordsClient _pwnedPasswordsClient;
        private const string TestPassword = "TestPassword";

        public PwnedPasswordsValidatorTests()
        {
            _pwnedPasswordsClient = Substitute.For<IPwnedPasswordsClient>();
            _pwnedPasswordsValidator = new PwnedPasswordsValidator(_pwnedPasswordsClient);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task ValidateAsync_PasswordIsNullOrWhitespace_ReturnsFalse(string password)
        {
            // Arrange
            _pwnedPasswordsClient.HasPasswordBeenPwnedAsync(password).Returns(Task.FromResult(false));

            // Act
            var isPwned = await _pwnedPasswordsValidator.ValidateAsync(password);

            //Assert
            isPwned.Should().BeFalse();
            await _pwnedPasswordsClient.DidNotReceive().HasPasswordBeenPwnedAsync(password);
        }

        [Fact]
        public async Task ValidateAsync_PasswordHasBeenPwned_ReturnsFalse()
        {
            // Arrange
            _pwnedPasswordsClient.HasPasswordBeenPwnedAsync(TestPassword).Returns(Task.FromResult(true));

            // Act
            var isPwned = await _pwnedPasswordsValidator.ValidateAsync(TestPassword);

            //Assert
            isPwned.Should().BeFalse();
        }

        [Fact]
        public async Task ValidateAsync_PasswordHasNotBeenPwned_ReturnsTrue()
        {
            // Arrange
            _pwnedPasswordsClient.HasPasswordBeenPwnedAsync(TestPassword).Returns(Task.FromResult(false));

            // Act
            var isPwned = await _pwnedPasswordsValidator.ValidateAsync(TestPassword);

            //Assert
            isPwned.Should().BeTrue();
        }

        [Fact]
        public async Task ValidateAsync_PasswordIsPassedToPasswordHasNotBeenPwned()
        {
            // Act
            await _pwnedPasswordsValidator.ValidateAsync(TestPassword);

            // Assert
            await _pwnedPasswordsClient.Received(1).HasPasswordBeenPwnedAsync(TestPassword);
            await _pwnedPasswordsClient.DidNotReceive().HasPasswordBeenPwnedAsync(Arg.Is<string>(x => !string.Equals(x, TestPassword)));
        }
    }
}
