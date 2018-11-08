using System;
using Core.Exceptions;
using Core.Registration;
using FluentAssertions;
using Xunit;

namespace WebAuth.Tests.Registration
{
    public class RegistrationModelTests
    {
        private const string ValidEmail = "test@test.com";
        private const string ComplexPassword = "QWEqwe123!";

        [Theory]
        [InlineData(ValidEmail)]
        [InlineData("invalid_email")]
        [InlineData("")]
        [InlineData(null)]
        public void Ctor_WhenEmailPassed_ModelIsCreated(string email)
        {
            var model = new RegistrationModel(email);

            model.Should().NotBeNull();
        }

        [Theory]
        [InlineData(ValidEmail)]
        public void Ctor_WhenEmailPassed_RegistrationIdIsGenerated(string email)
        {
            var model = new RegistrationModel(email);

            model.RegistrationId.Should().NotBeNull();
        }

        [Theory]
        [InlineData(ValidEmail)]
        public void Ctor_WhenEmailPassed_RegistrationIdLengthIs22(string email)
        {
            var model = new RegistrationModel(email);

            model.RegistrationId.Length.Should().Be(22);
        }

        [Theory]
        [InlineData(ValidEmail)]
        public void Ctor_WhenEmailPassed_RegistrationIdIsRandom(string email)
        {
            var model1 = new RegistrationModel(email);
            var model2 = new RegistrationModel(email);

            model1.RegistrationId.Should().NotBe(model2.RegistrationId);
        }

        [Theory]
        [InlineData(ValidEmail)]
        public void Ctor_WhenEmailPassed_FirstStepIsInitialInfo(string email)
        {
            var model = new RegistrationModel(email);

            model.CurrentStep.Should().Be(RegistrationStep.InitialInfo);
        }


        [Fact]
        public void SetInitialInfo_WhenEmailIsEqualToInitial_NoException()
        {
            var model = new RegistrationModel(ValidEmail);
            var initialInfoDto = new InitialInfoDto
            {
                Email = ValidEmail,
                Password = ComplexPassword,
                ClientId = "123"
            };

            model.CompleteInitialInfoStep(initialInfoDto);

            model.Should().NotBeNull();
        }

        [Fact]
        public void SetInitialInfo_WhenEmailIsDifferentFromInitial_ArgumentExceptionIsThrown()
        {
            var model = new RegistrationModel("email1@test.com");
            var initialInfoDto = new InitialInfoDto
            {
                Email = "email2@test.com",
                Password = ComplexPassword,
                ClientId = "123"
            };

            Action initialInfo = () => model.CompleteInitialInfoStep(initialInfoDto);

            initialInfo.Should().Throw<RegistrationEmailMatchingException>()
                .WithMessage("The email doesn't match to the one was provided during registration");
        }

        [Fact]
        public void SetInitialInfo_WhenClientIdPassed_FillsClientId()
        {
            var model = new RegistrationModel(ValidEmail);
            var clientId = "123";
            var initialInfoDto = new InitialInfoDto
            {
                Email = ValidEmail,
                Password = ComplexPassword,
                ClientId = clientId
            };

            model.CompleteInitialInfoStep(initialInfoDto);

            model.ClientId.Should().BeEquivalentTo(clientId);
        }

        [Fact]
        public void SetInitialInfo_WhenPasswordIsPassed_FillsSaltAndHash()
        {
            var model = new RegistrationModel(ValidEmail);
            var password = ComplexPassword;
            var initialInfoDto = new InitialInfoDto
            {
                Email = ValidEmail,
                Password = password,
                ClientId = "321"
            };

            model.CompleteInitialInfoStep(initialInfoDto);

            model.Salt.Should().NotBeEmpty();
            model.Hash.Should().NotBe(password);
        }

        [Theory]
        [InlineData("qweQWE123!")]
        [InlineData("12345%aA")]
        [InlineData("aA1!zzzz")]
        [InlineData("5FD924625F6AB16A19CC9807C7C506AE1813490E4BA675F843D5A10E0BAACDb!")]
        public void InitialInfo_WhenPasswordIsComplex_ShouldWork(string password)
        {
            var model = new RegistrationModel(ValidEmail);

            var initialInfoDto = new InitialInfoDto
            {
                Email = ValidEmail,
                Password = password,
                ClientId = "321"
            };

            model.CompleteInitialInfoStep(initialInfoDto);

            model.Should().NotBeNull();
        }

        [Theory]
        [InlineData("12345678")]
        [InlineData("1234567a")]
        [InlineData("123456aA")]
        [InlineData("aA1!")]
        [InlineData("aA1!zxc")]
        [InlineData("")]
        [InlineData(null)]
        public void InitialInfo_WhenPasswordIsNotComplex_ShouldThrowException(string password)
        {
            var model = new RegistrationModel(ValidEmail);

            var initialInfoDto = new InitialInfoDto
            {
                Email = ValidEmail,
                Password = password,
                ClientId = "321"
            };

            Action initialInfo = () => model.CompleteInitialInfoStep(initialInfoDto);

            initialInfo.Should().Throw<PasswordIsNotComplexException>();
        }

        [Fact]
        public void CanEmailBeUsed_WhenInitialInfoIsSet_ShouldBeFalse()
        {
            var model = new RegistrationModel(ValidEmail);

            var initialInfoDto = new InitialInfoDto
            {
                Email = ValidEmail,
                Password = ComplexPassword,
                ClientId = "321"
            };

            model.CompleteInitialInfoStep(initialInfoDto);

            model.CanEmailBeUsed().Should().BeFalse();
        }

        [Fact]
        public void CanEmailBeUsed_WhenInitialInfoNotSet_ShouldBeTrue()
        {
            var model = new RegistrationModel(ValidEmail);

            model.CanEmailBeUsed().Should().BeTrue();
        }
    }
}
