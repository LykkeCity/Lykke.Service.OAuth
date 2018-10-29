using System.Threading.Tasks;
using Core.PasswordValidation;
using JetBrains.Annotations;

namespace Lykke.Service.OAuth.Services.PasswordValidation.Validators
{
    /// <inheritdoc />
    [UsedImplicitly]
    public class PwnedPasswordsValidator : IPasswordValidator
    {
        private readonly IPwnedPasswordsClient _passwordsClient;

        public PwnedPasswordsValidator(IPwnedPasswordsClient passwordsClient)
        {
            _passwordsClient = passwordsClient;
        }
        
        /// <inheritdoc />
        public async Task<bool> ValidateAsync(string password)
        {
            var isPwned = await _passwordsClient.HasPasswordBeenPwnedAsync(password);
            return !isPwned;
        }
    }
}
