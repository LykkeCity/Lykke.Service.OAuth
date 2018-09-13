using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Core.Services
{
    /// <summary>
    /// Service for operations with OAuth/OpenId tokens.
    /// </summary>
    public interface ITokenService
    {
        /// <summary>
        ///     Removes refresh token from whitelist.
        /// </summary>
        /// <param name="refreshToken">Refresh token.</param>
        /// <returns>True if token was revoked.</returns>
        Task<bool> RevokeRefreshTokenAsync(string refreshToken);

        /// <summary>
        ///     Checks if refresh token exists in whitelist.
        /// </summary>
        /// <param name="refreshToken">Refresh token.</param>
        /// <returns>True if token is in whitelist.</returns>
        Task<bool> IsRefreshTokenInWhitelistAsync(string refreshToken);

        /// <summary>
        /// Replaces old token in whitelist with new one.
        /// </summary>
        /// <param name="oldRefreshToken">Old refresh token. If old token is null, only new refresh token is inserted.</param>
        /// <param name="newRefreshToken">New refresh token.</param>
        /// <returns>True if token was replaced.</returns>
        Task<bool> UpdateRefreshTokenInWhitelistAsync([CanBeNull]string oldRefreshToken, string newRefreshToken);
    }
}
