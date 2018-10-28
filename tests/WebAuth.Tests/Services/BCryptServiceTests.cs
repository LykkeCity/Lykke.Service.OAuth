using System;
using Core.Exceptions;
using Core.Services;
using Lykke.Logs;
using Lykke.Service.OAuth.Services;
using Xunit;

namespace WebAuth.Tests.Services
{
    public class BCryptServiceTests
    {
        private readonly IBCryptService _service;
        private const int WorkFactor = 10;
        private const string HelloHash = "$2y$10$bk3d5sthwvYbq./Vhmgzq.iumH7Lw3T0JEJfW3JN56JlyEdTIMXkC";
        private const string InconsistentHash = "inconsistentHash";
        private const string ValidHashWorkFactor9 = "$2y$09$7ptkSUgFFmJ6UQdzHyxbNOs86.8V5iXg5GfbDUGy151VJOX4ytDEG";
        private const string ValidHashWorkFactor10 = "$2y$10$M9UM/DSftpWy929/AmZHBusPYp5leHRkRLujVQfL1I1.3wlfDVSqa";
        private const string ValidHashWorkFactor11 = "$2y$11$WH3HgjCYbnsaVAV8Gv9WpuIeO7gpAZ0X6ANGY0MNfBVRCaqqPova6";
        private const string InvalidWorkFactorTokenHash = "$2y$200$WH3HgjCYbnsaVAV8Gv9WpuIeO7gpAZ0X6ANGY0MNfBVRCaqqPova6";
        private const string InvalidAlgorithmTokenHash = "$whatever$10$WH3HgjCYbnsaVAV8Gv9WpuIeO7gpAZ0X6ANGY0MNfBVRCaqqPova6";
        private const string InvalidHashTokenHash = "$2y$10$whatever";

        private const string ValidEmail = "whatever@whatever.com";
        private const string InvalidEmail = "any_email";

        public BCryptServiceTests()
        {
            _service = new BCryptService(WorkFactor, EmptyLogFactory.Instance);
        }

        [Theory]
        [InlineData(InvalidEmail, HelloHash)]
        public void Verify_InvalidEmail_ThrowsException(string email, string hash)
        {
            Assert.Throws<EmailHashInvalidException>(() => _service.Verify(email, hash));
        }

        [Theory]
        [InlineData(ValidEmail, InvalidWorkFactorTokenHash)]
        [InlineData(InvalidEmail, InvalidWorkFactorTokenHash)]
        [InlineData(ValidEmail, InvalidAlgorithmTokenHash)]
        [InlineData(InvalidEmail, InvalidAlgorithmTokenHash)]
        [InlineData(ValidEmail, InvalidHashTokenHash)]
        [InlineData(InvalidEmail, InvalidHashTokenHash)]
        public void Verify_InconsistentHash_ThrowsException(string email, string hash)
        {
            Assert.Throws<BCryptInternalException>(() => _service.Verify(email, hash));
        }

        [Theory]
        [InlineData(ValidEmail, InconsistentHash)]
        [InlineData(InvalidEmail, InconsistentHash)]
        public void Verify_HashInvalidFormat_ThrowsException(string email, string hash)
        {
            Assert.Throws<BCryptHashFormatException>(() => _service.Verify(email, hash));
        }

        [Theory]
        [InlineData(null, null)]
        [InlineData("", "")]
        [InlineData(" ", " ")]
        public void Verify_EmptyOrNullArguments_ThrowsException(string email, string hash)
        {
            Assert.Throws<ArgumentNullException>(() => _service.Verify(email, hash));
        }

        [Theory]
        [InlineData(ValidEmail, ValidHashWorkFactor9)]
        public void Verify_WorkFactorLess_ThrowsException(string email, string hash)
        {
            Assert.Throws<BCryptWorkFactorOutOfRangeException>(() => _service.Verify(email, hash));
        }

        [Theory]
        [InlineData(ValidEmail, ValidHashWorkFactor11)]
        public void Verify_WorkFactorOver_Success(string email, string hash)
        {
            var ex = Record.Exception(() => _service.Verify(email, hash));

            Assert.Null(ex);
        }

        [Theory]
        [InlineData(ValidEmail, ValidHashWorkFactor10)]
        public void Verify_ValidArguments_Success(string email, string hash)
        {
            var ex = Record.Exception(() => _service.Verify(email, hash));

            Assert.Null(ex);
        }
    }
}
