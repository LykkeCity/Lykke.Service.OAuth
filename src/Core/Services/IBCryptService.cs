using System;
using Core.Exceptions;

namespace Core.Services
{
    /// <summary>
    /// BCrypt related stuff service interface
    /// </summary>
    public interface IBCryptService
    {
        /// <summary>
        /// Validates source string against hash using work factor from settings
        /// </summary>
        /// <param name="source">Source string</param>
        /// <param name="hash">Source string bcrypt hash</param>
        /// <exception cref="EmailHashInvalidException">Thrown if hash is not valid for the source</exception>
        /// <exception cref="BCryptWorkFactorOutOfRangeException">Thrown if hash was calculated with different work factor than it is required</exception>
        /// <exception cref="BCryptInternalException">Thrown when there is an exception raised by BCrypt library</exception>
        /// <exception cref="ArgumentNullException">Thrown when arguments are null or empty</exception>
        /// <exception cref="BCryptHashFormatException">Thrown when hash string can't be parsed</exception>
        void Verify(string source, string hash);
    }
}
