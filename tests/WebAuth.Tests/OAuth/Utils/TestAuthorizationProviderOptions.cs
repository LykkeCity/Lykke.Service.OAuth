using Core.Application;
using Core.Services;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.Service.Session.Client;
using NSubstitute;
using WebAuth.Providers;

namespace WebAuth.Tests.OAuth.Utils
{
    /// <summary>
    ///     Class for configuring creation of test <see cref="AuthorizationProvider" />.
    /// </summary>
    internal class TestAuthorizationProviderOptions
    {
        [CanBeNull] internal IApplicationRepository ApplicationRepository { get; set; }
        [CanBeNull] internal IClientSessionsClient ClientSessionsClient { get; set; }
        [CanBeNull] internal ITokenService TokenService { get; set; }
        [CanBeNull] internal IValidationService ValidationService { get; set; }
        [CanBeNull] internal ILogFactory LogFactory { get; set; }

        public TestAuthorizationProviderOptions()
        {
            ApplicationRepository = Substitute.For<IApplicationRepository>();
            ClientSessionsClient = Substitute.For<IClientSessionsClient>();
            TokenService = Substitute.For<ITokenService>();
            ValidationService = Substitute.For<IValidationService>();
            LogFactory = Substitute.For<ILogFactory>();
        }
    }
}
