using System;
using Core.Registration;
using FluentAssertions;
using Lykke.Service.OAuth.Factories;
using Xunit;

namespace WebAuth.Tests.Registration
{
    public class RequestModelFactoryTests
    {
        public RequestModelFactoryTests()
        {
            _requestModelFactory = new RequestModelFactory();
        }

        private readonly RequestModelFactory _requestModelFactory;

        private const string ValidEmail = "test@test.com";
        private const string ComplexPassword = "QWEqwe123!";
        private readonly DateTime dateTime = new DateTime();

        [Fact]
        public void CreateForRegistrationService_WhenCountryIsIso2_ConvertsToIso3()
        {
            var registrationModel = new RegistrationModel(ValidEmail, dateTime);
            registrationModel.CompleteInitialInfoStep(new InitialInfoDto(){Email = ValidEmail, ClientId = "", Password =  ComplexPassword, Started = dateTime});
            registrationModel.CompleteAccountInfoStep(new AccountInfoDto(){ CountryCodeIso2 = "RU", FirstName = "test", LastName = "test", PhoneNumber = "23123"});

            var requestModel = _requestModelFactory.CreateForRegistrationService(registrationModel, "", "");

            requestModel.CountryFromPOA.Should().BeEquivalentTo("RUS");
        }
    }
}
