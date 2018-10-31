using System.Threading;
using System.Threading.Tasks;

namespace Core.PasswordValidation
{
    /// <summary>
    ///     Service for password validation.
    /// </summary>
    public interface IPasswordValidationService
    {
        /// <summary>
        ///     Checks if password is valid.
        /// </summary>
        /// <param name="password">The password to check.</param>
        /// <returns>
        ///     True if password is valid.
        ///     False otherwise.
        /// </returns>
        Task<bool> ValidateAsync(string password);
    }
}
