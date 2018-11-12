using System;
using System.Threading.Tasks;
using Core.Exceptions;

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

        /// <summary>
        ///     Checks if password is valid and throws exception if it is not
        /// </summary>
        /// <param name="password">The password to check.</param>
        /// <exception cref="PasswordIsEmptyException">Thrown when password is empty</exception>
        /// <exception cref="PasswordIsNotComplexException">Thrown when password is not complex enough</exception>
        /// <exception cref="PasswordIsPwnedException">Thrown when password has been pwned</exception>
        /// <exception cref="Exception">Thrown when password validation error code is unknown</exception>
        Task ValidateAndThrowAsync(string password);
    }
}
