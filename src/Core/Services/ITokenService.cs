using System.Threading.Tasks;
using Core.ExternalProvider.Exceptions;
using JetBrains.Annotations;

namespace Core.Services
{
    /// <summary>
    ///     Service for operations with OAuth/OpenId tokens.
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
        ///     Replaces old token in whitelist with new one.
        /// </summary>
        /// <param name="oldRefreshToken">Old refresh token. If old token is null, only new refresh token is inserted.</param>
        /// <param name="newRefreshToken">New refresh token.</param>
        /// <returns>True if token was replaced.</returns>
        Task UpdateRefreshTokenInWhitelistAsync([CanBeNull] string oldRefreshToken, string newRefreshToken);

        /// <summary>
        ///     Save ironclad refresh token.
        /// </summary>
        /// <param name="lykkeToken">Lykke token.</param>
        /// <param name="refreshToken">Ironclad refresh token.</param>
        /// <returns>Completed task if everything was successful.</returns>
        Task SaveIroncladRefreshTokenAsync(string lykkeToken, string refreshToken);

        /// <summary>
        ///     Get saved ironclad refresh token.
        /// </summary>
        /// <param name="lykkeToken">Lykke token.</param>
        /// <returns>Ironclad refresh token.</returns>
        /// <exception cref="TokenNotFoundException">Thrown when token not found.</exception>
        Task<string> GetIroncladRefreshTokenAsync(string lykkeToken);

        /// <summary>
        ///     Get saved ironclad access token.
        /// </summary>
        /// <param name="lykkeToken">Lykke token.</param>
        /// <returns>Ironclad access token.</returns>
        /// <exception cref="TokenNotFoundException">Thrown when token not found.</exception>
        Task<string> GetIroncladAccessTokenAsync(string lykkeToken);
    }
}
