using System;
using System.Threading.Tasks;
using Core.Services;
using Lykke.Service.ClientAccount.Client;
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

        public EmailValidationServiceTests()
        {
            InitMocks();

            _service = new EmailValidationService(_clientAccountClientMock.Object, _bCryptServiceMock.Object);
        }

        [Theory]
        [InlineData(null, null)]
        [InlineData("", "")]
        [InlineData(" ", " ")]
        public async Task IsEmailTaken_InvalidArguments_ThrowsException(string email, string hash)
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _service.IsEmailTakenAsync(email, hash));
        }

        private void InitMocks()
        {
            _bCryptServiceMock = new Mock<IBCryptService>();
            _clientAccountClientMock = new Mock<IClientAccountClient>();
        }
    }
}
