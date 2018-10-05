using System.Threading.Tasks;

namespace Core.Services
{
    /// <summary>
    ///     Service for OAuth process validation.
    /// </summary>
    public interface IValidationService
    {
        /// <summary>
        ///     Validates refresh token is valid(exists in whitelist).
        ///     And that session to which it was issued is alive.
        /// </summary>
        /// <param name="refreshToken">Refresh token.</param>
        /// <param name="sessionId">Session id to which token is issued to.</param>
        /// <returns>True if refresh token is valid.</returns>
        Task<bool> IsRefreshTokenValidAsync(string refreshToken, string sessionId);
    }
}
