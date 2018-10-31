using System;
using Core.Registration;
using FluentAssertions;
using Xunit;

namespace WebAuth.Tests.Registration
{
    public class RegistrationModelTests
    {
        private const string ValidEmail = "test@test.com";
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


        [Fact]
        public void SetInitialInfo_WhenEmailIsEqualToInitial_NoException()
        {
            var model = new RegistrationModel(ValidEmail);
            var registrationDto = new RegistrationDto
            {
                Email = ValidEmail,
                Password = "123",
                ClientId = "123"
            };

            model.SetInitialInfo(registrationDto);

            model.Should().NotBeNull();
        }

        [Fact]
        public void SetInitialInfo_WhenEmailIsDifferetFromInitial_ArgumentExceptionIsThrown()
        {
            var model = new RegistrationModel("email1@test.com");
            var registrationDto = new RegistrationDto
            {
                Email = "email2@test.com",
                Password = "123",
                ClientId = "123"
            };

            Assert.Throws<ArgumentException>(() => model.SetInitialInfo(registrationDto));
        }

        [Fact]
        public void SetInitialInfo_WhenClientIdPassed_FillsClientId()
        {
            var model = new RegistrationModel(ValidEmail);
            var clientId = "123";
            var registrationDto = new RegistrationDto
            {
                Email = ValidEmail,
                Password = "321",
                ClientId = clientId
            };

            model.SetInitialInfo(registrationDto);

            model.ClientId.Should().BeEquivalentTo(clientId);
        }

        [Fact]
        public void SetInitialInfo_WhenPasswordIsPassed_FillsSaltAndHash()
        {
            var model = new RegistrationModel(ValidEmail);
            var password = "123";
            var registrationDto = new RegistrationDto
            {
                Email = ValidEmail,
                Password = password,
                ClientId = "321"
            };

            model.SetInitialInfo(registrationDto);

            model.Salt.Should().NotBeEmpty();
            model.Hash.Should().NotBe(password);
        }
    }
}
