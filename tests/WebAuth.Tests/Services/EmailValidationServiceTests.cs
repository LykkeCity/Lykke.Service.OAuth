using System;
using System.Threading.Tasks;
using Core.Registration;
using Core.Services;
using FluentAssertions;
using Lykke.Service.ClientAccount.Client;
using Lykke.Service.ClientAccount.Client.Models;
using Lykke.Service.OAuth.Services;
using Moq;
using Xunit;

namespace WebAuth.Tests.Services
{
    public class EmailValidationServiceTests
    {
        private readonly IEmailValidationService _service;
        private Mock<IBCryptService> _bCryptServiceMock;
        private Mock<IClientAccountClient> _clientAccountClientMock;
        private Mock<IRegistrationRepository> _registrationRepo;
        private string _hash = "hash";

        public EmailValidationServiceTests()
        {
            InitMocks();

            _service = new EmailValidationService(_clientAccountClientMock.Object, _bCryptServiceMock.Object, _registrationRepo.Object);
        }

        [Theory]
        [InlineData(null, null)]
        [InlineData("", "")]
        [InlineData(" ", " ")]
        public async Task IsEmailTaken_InvalidArguments_ThrowsException(string email, string hash)
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _service.IsEmailTakenAsync(email, hash));
        }

        [Fact]
        public async Task IsEmailTaken_WhenEmailIsUsedInRegistration_ReturnsTrue()
        {
            var email = "test@test.com";
            var registrationModel = CreateRegistrationModel(email);
            _registrationRepo.Setup(x => x.TryGetByEmailAsync(email)).ReturnsAsync(registrationModel);

            var result = await _service.IsEmailTakenAsync(email, _hash);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task IsEmailTaken_WhenEmailIsUsedInRegistration_ClientAccountIsNotCalled()
        {
            var email = "test@test.com";
            var registrationModel = CreateRegistrationModel(email);
            _registrationRepo.Setup(x => x.TryGetByEmailAsync(email)).ReturnsAsync(registrationModel);

            var result = await _service.IsEmailTakenAsync(email, _hash);

            _clientAccountClientMock.Verify(
                x => x.IsTraderWithEmailExistsAsync(It.IsAny<string>(), null),
                Times.Never
            );
        }

        private static RegistrationModel CreateRegistrationModel(string email)
        {
            var registrationModel = new RegistrationModel(email);
            registrationModel.SetInitialInfo(new InitialInfoDto() {Email = email, Password = "zxcZXC123!"});
            return registrationModel;
        }

        [Fact]
        public async Task IsEmailTaken_WhenEmailNotFoundInRegistration_ClientAccountIsCalled()
        {
            var email = "test@test.com";

            var result = await _service.IsEmailTakenAsync(email, _hash);

            _clientAccountClientMock.Verify(
                x => x.IsTraderWithEmailExistsAsync(email, null),
                Times.Once
            );
        }

        [Fact]
        public async Task IsEmailTaken_WhenEmailTakenInClientAccount_ReturnsTrue()
        {
            var email = "test@test.com";
            _clientAccountClientMock.Setup(
                x => x.IsTraderWithEmailExistsAsync(It.IsAny<string>(), null)
            ).ReturnsAsync(new AccountExistsModel {IsClientAccountExisting = true} );

            var result = await _service.IsEmailTakenAsync(email, _hash);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task IsEmailTaken_WhenEmailNotTakenInClientAccount_ReturnsFalse()
        {
            var email = "test@test.com";
            _clientAccountClientMock.Setup(
                x => x.IsTraderWithEmailExistsAsync(It.IsAny<string>(), null)
            ).ReturnsAsync(new AccountExistsModel {IsClientAccountExisting = false} );

            var result = await _service.IsEmailTakenAsync(email, _hash);

            result.Should().BeFalse();
        }

        private void InitMocks()
        {
            _bCryptServiceMock = new Mock<IBCryptService>();
            _bCryptServiceMock.Setup(x => x.Verify(It.IsAny<string>(), It.IsAny<string>()));

            _clientAccountClientMock = new Mock<IClientAccountClient>();
            _clientAccountClientMock.Setup(
                x => x.IsTraderWithEmailExistsAsync(It.IsAny<string>(), null)
            ).ReturnsAsync(new AccountExistsModel
            {
                IsClientAccountExisting = false
            });

            _registrationRepo = new Mock<IRegistrationRepository>();
            _registrationRepo.Setup(x => x.TryGetByEmailAsync(It.IsAny<string>())).ReturnsAsync((RegistrationModel)null);
        }
    }
}
