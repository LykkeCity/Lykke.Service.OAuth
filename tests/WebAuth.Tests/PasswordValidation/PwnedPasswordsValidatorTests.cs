using System.Net.Http;
using System.Threading.Tasks;
using Lykke.Common.Log;
using Lykke.Service.OAuth.Extensions.PasswordValidation;
using Lykke.Service.OAuth.Services.PasswordValidation;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;

namespace WebAuth.Tests.PasswordValidation
{
    public class PwnedPasswordsValidatorTests
    {
        private const string Skip = "Manual testing only";

        [Fact(Skip = Skip)]
        public async Task HasPasswordBeenPwned_WhenStrongPassword_ReturnsFalse()
        {
            var service = GetClient();

            const string safePassword = "657ed4b7-954a-4777-92d7-eb887eb8025eaa43e773-9f62-42f6-b717-a15e6fef8751";

            var isPwned = await service.HasPasswordBeenPwnedAsync(safePassword);

            Assert.False(isPwned, "Checking for safe password should return false");
        }

        [Fact(Skip = Skip)]
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
    }
}
