using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Exceptions;
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

        /// <inheritdoc />
        public async Task ValidateAndThrowAsync(string password)
        {
            PasswordValidationResult result = await ValidateAsync(password);

            if (result.IsValid)
                return;

            switch (result.Error)
            {
                case PasswordValidationErrorCode.PasswordIsEmpty:
                    throw new PasswordIsEmptyException();
                case PasswordValidationErrorCode.PasswordIsNotComplex:
                    throw new PasswordIsNotComplexException();
                case PasswordValidationErrorCode.PasswordIsPwned:
                    throw new PasswordIsPwnedException();
                default:
                    throw new Exception(
                        $"Unexpected password validation error code = {result.Error.ToString()}");
            }
        }
    }
}
