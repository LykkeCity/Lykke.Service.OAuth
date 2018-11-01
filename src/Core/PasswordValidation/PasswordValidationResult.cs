using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.PasswordValidation
{
    /// <summary>
    ///     Result of password validation.
    /// </summary>
    public class PasswordValidationResult
    {
        /// <summary>
        ///     Indicates if validation was successful.
        /// </summary>
        public bool IsValid => !Errors.Any();

        /// <summary>
        ///     First error.
        /// </summary>
        public PasswordValidationErrorCode Error =>
            Errors.FirstOrDefault();

        /// <summary>
        ///     List of all Errors.
        /// </summary>
        public IEnumerable<PasswordValidationErrorCode> Errors { get; private set; }

        private PasswordValidationResult()
        {
        }

        /// <summary>
        ///     Create successful result.
        /// </summary>
        /// <returns>Result with empty list of errors.</returns>
        public static PasswordValidationResult Success()
        {
            return new PasswordValidationResult
            {
                Errors = new List<PasswordValidationErrorCode>()
            };
        }

        /// <summary>
        ///     Create failed result.
        /// </summary>
        /// <returns>Result with one error.</returns>
        public static PasswordValidationResult Fail(PasswordValidationErrorCode error)
        {
            return new PasswordValidationResult
            {
                Errors = new List<PasswordValidationErrorCode> {error}
            };
        }

        /// <summary>
        ///     Create failed result.
        /// </summary>
        /// <returns>Result with multiple errors.</returns>
        public static PasswordValidationResult Fail(IEnumerable<PasswordValidationErrorCode> errors)
        {
            if (errors == null)
                throw new ArgumentNullException(nameof(errors));

            return new PasswordValidationResult
            {
                Errors = errors
            };
        }
    }
}
