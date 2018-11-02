namespace Core.PasswordValidation
{
    /// <summary>
    ///     Possible error codes during password validation.
    /// </summary>
    public enum PasswordValidationErrorCode
    {
        /// <summary>
        ///     Password is null or whitespace.
        /// </summary>
        PasswordIsEmpty,

        /// <summary>
        ///     Password was compromised earlier.
        /// </summary>
        PasswordIsPwned
    }
}
