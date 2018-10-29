using System;
using Core.Exceptions;

namespace Core.Extensions
{
    /// <summary>
    /// Extensions methods for bcrypt stuff
    /// </summary>
    public static class BCryptExtensions
    {
        /// <summary>
        /// BCrypt hash tokens separator
        /// </summary>
        public static char BCryptTokenSeparator = '$';

        /// <summary>
        /// Extracts bCrypt work factor from hash string
        /// </summary>
        /// <param name="src">The hash string</param>
        /// <returns>Work factor value</returns>
        /// <exception cref="ArgumentNullException">Thrown when hash is null or empty</exception>
        /// <exception cref="BCryptHashFormatException">Thrown when hash string can't be parsed</exception>
        public static int ExtractWorkFactor(this string src)
        {
            if (string.IsNullOrWhiteSpace(src))
                throw new ArgumentNullException(nameof(src));

            try
            {
                return Convert.ToInt16(src.Split(BCryptTokenSeparator)[2]);
            }
            catch (Exception)
            {
                throw new BCryptHashFormatException(hash: src);
            }
        }
    }
}
