using System.Threading.Tasks;
using Core.Services;
using JetBrains.Annotations;
using Lykke.Service.Session.Client;

namespace Lykke.Service.OAuth.Services
{
    [UsedImplicitly]
    public class ValidationService : IValidationService
    {
        private readonly IClientSessionsClient _clientSessionsClient;
        private readonly ITokenService _tokenService;

        public ValidationService(
            IClientSessionsClient clientSessionsClient,
            ITokenService tokenService)
        {
            _clientSessionsClient = clientSessionsClient;
            _tokenService = tokenService;
        }

        /// <inheritdoc />
        public async Task<bool> IsRefreshTokenValidAsync(string refreshToken, string sessionId)
        {
            if (string.IsNullOrWhiteSpace(sessionId) || string.IsNullOrWhiteSpace(refreshToken))
                return false;

            var isRefreshTokenExist = await _tokenService.IsRefreshTokenInWhitelistAsync(refreshToken);

            if (!isRefreshTokenExist)
                return false;

            // Check if session is alive.
            var session = await _clientSessionsClient.GetAsync(sessionId);

            if (session != null) return true;

            // If session was revoked we should revoke refresh_token too.
            await _tokenService.RevokeRefreshTokenAsync(refreshToken);
            return false;
        }
    }
}
