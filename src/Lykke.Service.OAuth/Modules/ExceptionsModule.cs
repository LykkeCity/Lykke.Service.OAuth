using System.Net;
using Autofac;
using Core.Exceptions;
using Lykke.Service.OAuth.ApiErrorCodes;

namespace Lykke.Service.OAuth.Modules
{
    /// <summary>
    /// Autofac module for registering exceptions handling configuration
    /// </summary>
    public class ExceptionsModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            var config = new ExceptionsHandlingConfiguration()

                .AddWarning(typeof(EmailHashInvalidException), HttpStatusCode.BadRequest,
                    RegistrationErrorCodes.InvalidBCryptHash)

                .AddWarning(typeof(BCryptWorkFactorOutOfRangeException), HttpStatusCode.BadRequest,
                    RegistrationErrorCodes.BCryptWorkFactorOutOfRange)

                .AddWarning(typeof(BCryptInternalException), HttpStatusCode.BadRequest,
                    RegistrationErrorCodes.BCryptInternalError)

                .AddWarning(typeof(BCryptHashFormatException), HttpStatusCode.BadRequest,
                    RegistrationErrorCodes.InvalidBCryptHashFormat)

                .AddNoLog(typeof(RegistrationKeyNotFoundException), HttpStatusCode.NotFound,
                    RegistrationErrorCodes.RegistrationNotFound)

                .AddError(typeof(InvalidPhoneNumberFormatException), HttpStatusCode.BadRequest,
                    RegistrationErrorCodes.InvalidPhoneFormat)

                .AddError(typeof(PhoneNumberAlreadyInUseException), HttpStatusCode.BadRequest,
                    RegistrationErrorCodes.PhoneNumberInUse)

                .AddError(typeof(CountryFromRestrictedListException), HttpStatusCode.BadRequest,
                    RegistrationErrorCodes.CountryFromRestrictedList)

                .AddError(typeof(CountryInvalidException), HttpStatusCode.BadRequest,
                    RegistrationErrorCodes.CountryCodeInvalid)

                .AddNoLog(typeof(PasswordIsNotComplexException), HttpStatusCode.BadRequest,
                    PasswordValidationApiErrorCodes.PasswordIsNotComplex)

                .AddError(typeof(ClientNotFoundException), HttpStatusCode.NotFound,
                    OAuthErrorCodes.ClientNotFound)

                .AddNoLog(typeof(PasswordIsEmptyException), HttpStatusCode.BadRequest,
                    PasswordValidationApiErrorCodes.PasswordIsEmpty)

                .AddNoLog(typeof(PasswordIsPwnedException), HttpStatusCode.BadRequest,
                    PasswordValidationApiErrorCodes.PasswordIsPwned);

            builder.Register(c => config)
                .AsSelf()
                .SingleInstance();
        }
    }
}
