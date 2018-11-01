using System.Threading.Tasks;
using Core.PasswordValidation;

namespace Lykke.Service.OAuth.Services.PasswordValidation.Validators
{
    /// <inheritdoc />
    public class PwnedPasswordsValidator : IPasswordValidator
    {
        private readonly IPwnedPasswordsClient _passwordsClient;

        public PwnedPasswordsValidator(IPwnedPasswordsClient passwordsClient)
        {
            _passwordsClient = passwordsClient;
        }

        /// <inheritdoc />
        public async Task<PasswordValidationResult> ValidateAsync(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return PasswordValidationResult.Fail(PasswordValidationErrorCode.PasswordIsEmpty);

            var isPwned = await _passwordsClient.HasPasswordBeenPwnedAsync(password);

            return isPwned
                ? PasswordValidationResult.Fail(PasswordValidationErrorCode.PasswordIsPwned)
                : PasswordValidationResult.Success();
        }
    }
}
