using System.Threading.Tasks;

namespace Core.PasswordValidation
{
    /// <summary>
    ///     Interface for password validator.
    /// </summary>
    public interface IPasswordValidator
    {
        /// <summary>
        ///     Asynchronously validate password.
        /// </summary>
        /// <param name="password">The password to check.</param>
        /// <returns>
        ///     True if password is valid.
        ///     False otherwise.
        /// </returns>
        Task<bool> ValidateAsync(string password);
    }
}
