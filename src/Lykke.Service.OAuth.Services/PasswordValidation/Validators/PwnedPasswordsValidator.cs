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
        public async Task<bool> ValidateAsync(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return false;

            var isPwned = await _passwordsClient.HasPasswordBeenPwnedAsync(password);
            
            return !isPwned;
        }
    }
}
