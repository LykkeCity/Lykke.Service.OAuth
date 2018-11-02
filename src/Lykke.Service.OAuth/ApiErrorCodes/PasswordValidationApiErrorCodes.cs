using System.Collections.Generic;
using Core.PasswordValidation;
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

        private static readonly IDictionary<PasswordValidationErrorCode, ILykkeApiErrorCode> ApiErrorCodesMap =
            new Dictionary<PasswordValidationErrorCode, ILykkeApiErrorCode>
            {
                {PasswordValidationErrorCode.PasswordIsEmpty, PasswordIsEmpty},
                {PasswordValidationErrorCode.PasswordIsPwned, PasswordIsPwned},
                {PasswordValidationErrorCode.PasswordIsNotComplex, PasswordIsNotComplex}
            };

        /// <summary>
        ///     Get Api error code by password validation error code.
        /// </summary>
        /// <param name="validationErrorCode">Error code returned by password validation.</param>
        /// <returns>Api error code.</returns>
        public static ILykkeApiErrorCode GetApiErrorCodeByValidationErrorCode(
            PasswordValidationErrorCode validationErrorCode)
        {
            return ApiErrorCodesMap[validationErrorCode];
        }
    }
}
