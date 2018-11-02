using System;
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
            trueValidator.ValidateAsync(TestPassword).Returns(Task.FromResult(PasswordValidationResult.Success()));

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
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public async Task ValidateAsync_OneValidatorReturnsFalse_ReturnsFalse()
        {
            // Arrange
            var trueValidator = Substitute.For<IPasswordValidator>();
            trueValidator.ValidateAsync(TestPassword).Returns(Task.FromResult(PasswordValidationResult.Success()));         
            
            var falseValidator = Substitute.For<IPasswordValidator>();
            falseValidator.ValidateAsync(TestPassword).Returns(Task.FromResult(PasswordValidationResult.Fail(PasswordValidationErrorCode.PasswordIsEmpty)));

            var validators = new List<IPasswordValidator>
            {
                falseValidator,
                trueValidator
            };

            var validationService = new PasswordValidationService(validators);

            // Act
            var result = await validationService.ValidateAsync(TestPassword);

            // Assert
            result.IsValid.Should().BeFalse();
        }

        [Fact]
        public void ValidateAsync_ValidatorsAreEmptyList_ThrowsArgumentException()
        {
            // Arrange
            var validationService = new PasswordValidationService(new List<IPasswordValidator>());

            // Act
            Func<Task> action = () => validationService.ValidateAsync(TestPassword);

            // Assert
            action.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void ValidateAsync_ValidatorsAreNull_ThrowsArgumentException()
        {
            // Arrange
            var validationService = new PasswordValidationService(null);

            // Act
            Func<Task> action = () => validationService.ValidateAsync(TestPassword);

            // Assert
            action.Should().Throw<ArgumentException>();
        }

        [Fact]
        public async Task ValidateAsync_PasswordIsPassedToValidators()
        {
            // Arrange
            var trueValidator = Substitute.For<IPasswordValidator>();
            trueValidator.ValidateAsync(TestPassword).Returns(Task.FromResult(PasswordValidationResult.Success()));         
            
            var falseValidator = Substitute.For<IPasswordValidator>();
            falseValidator.ValidateAsync(TestPassword).Returns(Task.FromResult(PasswordValidationResult.Fail(PasswordValidationErrorCode.PasswordIsEmpty)));

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
