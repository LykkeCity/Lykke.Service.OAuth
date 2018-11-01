using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.PasswordValidation;

namespace Lykke.Service.OAuth.Services.PasswordValidation
{
    /// <inheritdoc />
    public class PasswordValidationService : IPasswordValidationService
    {
        private readonly IEnumerable<IPasswordValidator> _passwordValidators;

        public PasswordValidationService(
            IEnumerable<IPasswordValidator> passwordValidators)
        {
            _passwordValidators = passwordValidators;
        }

        /// <inheritdoc />
        public async Task<PasswordValidationResult> ValidateAsync(string password)
        {
            if (_passwordValidators == null || !_passwordValidators.Any())
                throw new ArgumentException(nameof(_passwordValidators));

            var tasks = _passwordValidators.Select(validator => validator.ValidateAsync(password));

            var validationResults = await Task.WhenAll(tasks);

            var errors = validationResults.SelectMany(result => result.Errors).Distinct().ToList();

            return errors.Any()
                ? PasswordValidationResult.Fail(errors)
                : PasswordValidationResult.Success();
        }
    }
}
