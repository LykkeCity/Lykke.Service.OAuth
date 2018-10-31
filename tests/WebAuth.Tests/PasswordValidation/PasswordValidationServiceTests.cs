using System.Collections.Generic;
using System.Threading.Tasks;
using Core.PasswordValidation;
using FluentAssertions;
using Lykke.Service.OAuth.Services.PasswordValidation;
using NSubstitute;
using Xunit;

namespace WebAuth.Tests.PasswordValidation
{
    public class PasswordValidationServiceTests
    {
        private const string TestPassword = "TestPassword";

        [Fact]
        public async Task ValidateAsync_AllValidatorsReturnTrue_ReturnsTrue()
        {
            // Arrange
            var trueValidator = Substitute.For<IPasswordValidator>();
            trueValidator.ValidateAsync(TestPassword).Returns(Task.FromResult(true));

            var validators = new List<IPasswordValidator>
            {
                trueValidator,
                trueValidator,
                trueValidator
            };

            var validationService = new PasswordValidationService(validators);

            // Act
            var result = await validationService.ValidateAsync(TestPassword);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task ValidateAsync_OneValidatorReturnsFalse_ReturnsFalse()
        {
            // Arrange
            var trueValidator = Substitute.For<IPasswordValidator>();
            trueValidator.ValidateAsync(TestPassword).Returns(Task.FromResult(true));         
            
            var falseValidator = Substitute.For<IPasswordValidator>();
            falseValidator.ValidateAsync(TestPassword).Returns(Task.FromResult(false));

            var validators = new List<IPasswordValidator>
            {
                falseValidator,
                trueValidator
            };

            var validationService = new PasswordValidationService(validators);

            // Act
            var result = await validationService.ValidateAsync(TestPassword);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task ValidateAsync_ValidatorsAreEmptyList_ReturnsFalse()
        {
            // Arrange
            var validationService = new PasswordValidationService(new List<IPasswordValidator>());

            // Act
            var result = await validationService.ValidateAsync(TestPassword);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task ValidateAsync_ValidatorsAreNull_ReturnsFalse()
        {
            // Arrange
            var validationService = new PasswordValidationService(null);

            // Act
            var result = await validationService.ValidateAsync(TestPassword);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task ValidateAsync_PasswordIsPassedToValidators()
        {
            // Arrange
            var trueValidator = Substitute.For<IPasswordValidator>();
            trueValidator.ValidateAsync(TestPassword).Returns(Task.FromResult(true));         
            
            var falseValidator = Substitute.For<IPasswordValidator>();
            falseValidator.ValidateAsync(TestPassword).Returns(Task.FromResult(false));

            var validators = new List<IPasswordValidator>
            {
                falseValidator,
                trueValidator
            };

            var validationService = new PasswordValidationService(validators);

            // Act
            await validationService.ValidateAsync(TestPassword);

            // Assert
            await trueValidator.Received(1).ValidateAsync(TestPassword);
            await trueValidator.DidNotReceive().ValidateAsync(Arg.Is<string>(x => !string.Equals(x, TestPassword)));

            await falseValidator.Received(1).ValidateAsync(TestPassword);
            await falseValidator.DidNotReceive().ValidateAsync(Arg.Is<string>(x => !string.Equals(x, TestPassword)));
        }
    }
}
