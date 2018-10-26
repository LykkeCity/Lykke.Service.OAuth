using System;

namespace Core.Extensions
{
    public static class BCryptExtensions
    {
        public static char BCryptTokenSeparator = '$';

        /// <summary>
        /// Extracts bCrypt work factor from hash string
        /// </summary>
        /// <param name="src">The hash string</param>
        /// <returns>Work factor value</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static int ExtractWorkFactor(this string src)
        {
            if (string.IsNullOrWhiteSpace(src))
                throw new ArgumentNullException(nameof(src));

            return Convert.ToInt16(src.Split(BCryptTokenSeparator)[2]);
        }
    }
}
