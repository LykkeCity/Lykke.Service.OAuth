using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.PasswordValidation;
using JetBrains.Annotations;

namespace Lykke.Service.OAuth.Services.PasswordValidation
{
    /// <inheritdoc />
    [UsedImplicitly]
    public class PasswordValidationService : IPasswordValidationService
    {
        private readonly IEnumerable<IPasswordValidator> _passwordValidators;

        public PasswordValidationService(
            [NotNull] IEnumerable<IPasswordValidator> passwordValidators)
        {
            _passwordValidators = passwordValidators;
        }

        /// <inheritdoc />
        public async Task<bool> ValidateAsync(string password)
        {
            if (_passwordValidators == null || !_passwordValidators.Any())
                return false;

            var validationResults = _passwordValidators.Select(validator => validator.ValidateAsync(password));

            var validationValues = await Task.WhenAll(validationResults);

            return validationValues.All(isValid => isValid);
        }
    }
}
