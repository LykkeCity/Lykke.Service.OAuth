using System;
using Core.Exceptions;
using Core.PasswordValidation;

namespace Core
{
    /// <summary>
    /// Extension methods for <see cref="PasswordValidationResult"/>
    /// </summary>
    public static class PasswordValidationResultExtensions
    {
        /// <summary>
        /// Throws exception if password is not valid, otherwise does nothing
        /// </summary>
        /// <param name="src"></param>
        /// <exception cref="ArgumentNullException">Thrown when validation result is null</exception>
        /// <exception cref="PasswordIsEmptyException">Thrown when password is empty</exception>
        /// <exception cref="PasswordIsNotComplexException">Thrown when password is not complex enough</exception>
        /// <exception cref="PasswordIsPwnedException">Thrown when password has been pwned</exception>
        /// <exception cref="Exception">Thrown when password validation error code is unknown</exception>
        public static void ThrowOrKeepSilent(this PasswordValidationResult src)
        {
            if (src == null)
                throw new ArgumentNullException(nameof(src));

            if (src.IsValid)
                return;

            switch (src.Error)
            {
                case PasswordValidationErrorCode.PasswordIsEmpty:
                    throw new PasswordIsEmptyException();
                case PasswordValidationErrorCode.PasswordIsNotComplex:
                    throw new PasswordIsNotComplexException();
                case PasswordValidationErrorCode.PasswordIsPwned:
                    throw new PasswordIsPwnedException();
                default:
                    throw new Exception($"Unexpected password validation error code = {src.Error.ToString()}");
            }
        }
    }
}
