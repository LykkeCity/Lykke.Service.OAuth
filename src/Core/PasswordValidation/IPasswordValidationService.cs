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
        ///     <see cref="PasswordValidationResult"/> with no errors if password is valid.
        ///     <see cref="PasswordValidationResult"/> with errors otherwise.
        /// </returns>
        Task<PasswordValidationResult> ValidateAsync(string password);
    }
}
