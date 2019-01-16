using System.Threading.Tasks;
using Core.ExternalProvider;
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
        ///     Save ironclad openid tokens, associated by lykke token.
        /// </summary>
        /// <param name="lykkeToken">lykke token</param>
        /// <param name="tokens">tokens from ironclad</param>
        /// <returns>completed task upon success</returns>
        Task SaveIroncladTokensAsync(string lykkeToken, OpenIdTokens tokens);

        /// <summary>
        ///     Get saved ironclad openid tokens.
        ///     Refreshes them if expired.
        /// </summary>
        /// <param name="lykkeToken">lykke token</param>
        /// <returns>Openid tokens</returns>
        Task<OpenIdTokens> GetFreshIroncladTokens(string lykkeToken);

        /// <summary>
        ///     Revoke access and refresh tokens.
        /// </summary>
        /// <param name="tokens">ironclad openid tokens</param>
        /// <returns>completed task upon success</returns>
        Task RevokeIroncladTokensAsync(OpenIdTokens tokens);

        /// <summary>
        ///     Get saved ironclad openid tokens without refresh.
        /// </summary>
        /// <param name="lykkeToken">lykke token</param>
        /// <returns>ironclad openid tokens</returns>
        Task<OpenIdTokens> GetIroncladTokens(string lykkeToken);

        /// <summary>
        ///     Remove saved ironclad openid tokens.
        /// </summary>
        /// <param name="lykkeToken">lykke token</param>
        /// <returns>True if tokens were deleted, false if they did not exist.</returns>
        Task<bool> DeleteIroncladTokens(string lykkeToken);
    }
}
