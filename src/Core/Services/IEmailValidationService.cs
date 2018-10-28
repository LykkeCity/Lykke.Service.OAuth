using System;
using System.Threading.Tasks;
using Core.Exceptions;

namespace Core.Services
{
    /// <summary>
    /// Email validation service interface
    /// </summary>
    public interface IEmailValidationService
    {
        /// <summary>
        /// Checks if email already used
        /// </summary>
        /// <param name="email">The email to validate</param>
        /// <param name="hash">Email hash</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown when either email or hash is null or empty</exception>
        /// <exception cref="EmailHashInvalidException">Thrown if hash is not valid for the source</exception>
        /// <exception cref="BCryptWorkFactorOutOfRangeException">Thrown if hash was calculated with different work factor than it is required</exception>
        /// <exception cref="BCryptInternalException">Thrown when there is an exception raised by BCrypt library</exception>
        /// <exception cref="BCryptHashFormatException">Thrown when hash string can't be parsed</exception>
        Task<bool> IsEmailTakenAsync(string email, string hash);
    }
}
