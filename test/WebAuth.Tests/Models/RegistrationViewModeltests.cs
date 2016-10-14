using WebAuth.Models;
using Xunit;

namespace WebAuth.Tests.Models
{
    public class RegistrationViewModelTests
    {
        [Fact]
        public void Invalid_Model_ThrowValidationErrors()
        {
            //arrange
            var model = new RegistrationViewModel();

            //act
            var results = TestModelHelper.Validate(model);

            //assert
            Assert.Equal(3, results.Count);
        }

        [Fact]
        public void Valid_Model_ThrowNoValidationErrors()
        {
            //arrange
            var model = new RegistrationViewModel
            {
                Email = "test@test.com",
                RegistrationPassword = "123456",
                ConfirmPassword = "123456"
            };

            //act
            var results = TestModelHelper.Validate(model);

            //assert
            Assert.Equal(0, results.Count);
        }

        [Fact]
        public void InValid_Passwords_ThrowValidationErrors()
        {
            //arrange
            var model = new RegistrationViewModel
            {
                Email = "test@test.com",
                RegistrationPassword = "123456",
                ConfirmPassword = "111"
            };

            //act
            var results = TestModelHelper.Validate(model);

            //assert
            Assert.Equal(1, results.Count);
        }
    }
}