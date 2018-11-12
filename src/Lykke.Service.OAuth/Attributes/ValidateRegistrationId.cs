using System;
using System.ComponentModel.DataAnnotations;
using Lykke.Common.ApiLibrary.Exceptions;
using Lykke.Service.OAuth.ApiErrorCodes;

namespace Lykke.Service.OAuth.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
    internal class ValidateRegistrationId : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var registrationId = value as string;

            if(string.IsNullOrWhiteSpace(registrationId))
                throw LykkeApiErrorException.NotFound(RegistrationErrorCodes.RegistrationNotFound);

            return ValidationResult.Success;
        }
    }
}
