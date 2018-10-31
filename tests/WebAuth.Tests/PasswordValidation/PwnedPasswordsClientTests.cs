using System;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Flurl.Http.Testing;
using Lykke.Common.Log;
using Lykke.Service.OAuth.Extensions.PasswordValidation;
using Lykke.Service.OAuth.Services.PasswordValidation;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;

namespace WebAuth.Tests.PasswordValidation
{
    public class PwnedPasswordsClientTests : IDisposable
    {
        private readonly ILogFactory _fakeLogFactory;
        private readonly HttpTest _httpTest;
        private const string SkipReason = "Manual testing only";
        private const string TestPassword = "TestPassword";

        public PwnedPasswordsClientTests()
        {
            _fakeLogFactory = Substitute.For<ILogFactory>();
            _httpTest = new HttpTest();
        }

        //public async Task HasPasswordBeenPwned_ClientThrowsException_ReturnsTrue()

        [Fact]
        public async Task HasPasswordBeenPwned_ClientReturnsNot200StatusCode_ReturnsTrue()
        {
            // Arrange
            using (var fakeHttpClient = new HttpClient(new FakeHttpMessageHandler()))
            {
                fakeHttpClient.BaseAddress = new Uri("http://localhost");
                _httpTest.RespondWith("", 404);

                var service = new PwnedPasswordsClient(_fakeLogFactory, fakeHttpClient);

                // Act
                var isPwned = await service.HasPasswordBeenPwnedAsync(TestPassword);

                // Assert
                isPwned.Should().BeTrue();
            }
        }

        [Fact]
        public async Task HasPasswordBeenPwned_ExceptionThrown_ReturnsTrue()
        {
            // Arrange
            using (var fakeHttpClient = new HttpClient(new ThrowExceptionMessageHandler()))
            {
                fakeHttpClient.BaseAddress = new Uri("http://localhost");

                var service = new PwnedPasswordsClient(_fakeLogFactory, fakeHttpClient);

                // Act
                var isPwned = await service.HasPasswordBeenPwnedAsync(TestPassword);

                // Assert
                isPwned.Should().BeTrue();
            }
        }

        [Fact(Skip = SkipReason)]
        public async Task HasPasswordBeenPwned_WhenStrongPassword_ReturnsFalse()
        {
            var service = GetClient();

            const string safePassword = "657ed4b7-954a-4777-92d7-eb887eb8025eaa43e773-9f62-42f6-b717-a15e6fef8751";

            var isPwned = await service.HasPasswordBeenPwnedAsync(safePassword);

            Assert.False(isPwned, "Checking for safe password should return false");
        }

        [Fact(Skip = SkipReason)]
        public async Task HasPasswordBeenPwned_WhenWeakPassword_ReturnsTrue()
        {
            var service = GetClient();

            const string pwnedPassword = "password";

            var isPwned = await service.HasPasswordBeenPwnedAsync(pwnedPassword);

            Assert.True(isPwned, "Checking for Pwned password should return true");
        }

        private static PwnedPasswordsClient GetClient()
        {
            var services = new ServiceCollection();

            services.AddPwnedPasswordHttpClient();

            var provider = services.BuildServiceProvider();

            var logFactory = Substitute.For<ILogFactory>();

            var service = new PwnedPasswordsClient(
                logFactory,
                provider.GetService<IHttpClientFactory>().CreateClient(PwnedPasswordsClient.HttpClientName));

            return service;
        }

        public void Dispose()
        {
            _httpTest.Dispose();
        }
    }
}
