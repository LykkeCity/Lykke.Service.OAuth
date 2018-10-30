using System.Threading;
using System.Threading.Tasks;

namespace Core.PasswordValidation
{
    /// <summary>
    /// A client for communicating with Troy Hunt's HaveIBeenPwned API
    /// </summary>
    public interface IPwnedPasswordsClient
    {
        /// <summary>
        /// Checks if a provided password has appeared in a known data breach
        /// </summary>
        /// <param name="password">The password to check</param>
        /// <param name="cancellationToken">An optional cancellation token</param>
        /// <returns>
        /// Returns True is password has been compromised.
        /// Returns False if password is strong and has not been compromised.
        /// </returns>
        Task<bool> HasPasswordBeenPwnedAsync(string password, CancellationToken cancellationToken = default);
    }
}
