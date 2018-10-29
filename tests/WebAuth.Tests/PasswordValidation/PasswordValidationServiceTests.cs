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
        private readonly IPasswordValidator _trueValidator;
        private readonly IPasswordValidator _falseValidator;

        public PasswordValidationServiceTests()
        {
            _trueValidator = Substitute.For<IPasswordValidator>();
            _trueValidator.ValidateAsync("").ReturnsForAnyArgs(Task.FromResult(true));

            _falseValidator = Substitute.For<IPasswordValidator>();
            _falseValidator.ValidateAsync("").ReturnsForAnyArgs(Task.FromResult(false));
        }

        [Fact]
        public async Task ValidateAsync_AllValidatorsReturnTrue_ReturnsTrue()
        {
            // Arrange
            var trueValidator = Substitute.For<IPasswordValidator>();
            trueValidator.ValidateAsync("").ReturnsForAnyArgs(Task.FromResult(true));

            var validators = new List<IPasswordValidator>
            {
                trueValidator,
                trueValidator,
                trueValidator
            };


            var validationService = new PasswordValidationService(validators);

            // Act
            var result = await validationService.ValidateAsync("123");

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task ValidateAsync_OneValidatorReturnsFalse_ReturnsFalse()
        {
            // Arrange
            var validators = new List<IPasswordValidator>
            {
                _falseValidator,
                _trueValidator
            };

            var validationService = new PasswordValidationService(validators);

            // Act
            var result = await validationService.ValidateAsync("123");

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task ValidateAsync_ValidatorsAreEmptyList_ReturnsFalse()
        {
            // Arrange
            var validators = new List<IPasswordValidator>();

            var validationService = new PasswordValidationService(validators);

            // Act
            var result = await validationService.ValidateAsync("123");

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task ValidateAsync_ValidatorsAreNull_ReturnsFalse()
        {
            // Arrange
            var validationService = new PasswordValidationService(null);

            // Act
            var result = await validationService.ValidateAsync("123");

            // Assert
            result.Should().BeFalse();
        }
    }
}
