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
        /// <exception cref="BCryptWorkFactorInvalidException">Thrown if hash was calculated with different work factor than it is required</exception>
        void Verify(string source, string hash);
    }
}
