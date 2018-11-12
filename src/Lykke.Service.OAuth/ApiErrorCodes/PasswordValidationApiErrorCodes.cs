using Lykke.Common.ApiLibrary.Contract;

namespace Lykke.Service.OAuth.ApiErrorCodes
{
    /// <summary>
    ///     Class for password validation error codes.
    /// </summary>
    public static class PasswordValidationApiErrorCodes
    {
        /// <summary>
        ///     Password is empty.
        /// </summary>
        public static readonly ILykkeApiErrorCode PasswordIsEmpty =
            new LykkeApiErrorCode(nameof(PasswordIsEmpty), "Password is empty.");

        /// <summary>
        ///     Password was compromised earlier.
        /// </summary>
        public static readonly ILykkeApiErrorCode PasswordIsPwned =
            new LykkeApiErrorCode(nameof(PasswordIsPwned), "Password have been exposed in data breaches.");

        /// <summary>
        ///     Password was compromised earlier.
        /// </summary>
        public static readonly ILykkeApiErrorCode PasswordIsNotComplex =
            new LykkeApiErrorCode(nameof(PasswordIsNotComplex), "Password is not complex enough.");
    }
}
