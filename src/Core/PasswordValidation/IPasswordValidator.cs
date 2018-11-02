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
        ///     <see cref="PasswordValidationResult"/> with no errors if password is valid.
        ///     <see cref="PasswordValidationResult"/> with errors otherwise.
        /// </returns>
        Task<PasswordValidationResult> ValidateAsync(string password);
    }
}
