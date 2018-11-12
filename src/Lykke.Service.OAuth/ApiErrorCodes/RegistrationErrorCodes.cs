using Lykke.Common.ApiLibrary.Contract;

namespace Lykke.Service.OAuth.ApiErrorCodes
{
    public static class RegistrationErrorCodes
    {
        public static readonly ILykkeApiErrorCode RegistrationNotFound =
            new LykkeApiErrorCode(nameof(RegistrationNotFound), "Registration id was not found");

        public static readonly ILykkeApiErrorCode InvalidBCryptHashFormat =
            new LykkeApiErrorCode(nameof(InvalidBCryptHashFormat), "BCrypt hash format is not valid");

        public static readonly ILykkeApiErrorCode InvalidBCryptHash =
            new LykkeApiErrorCode(nameof(InvalidBCryptHash), "BCrypt hash is not valid");

        public static readonly ILykkeApiErrorCode BCryptWorkFactorOutOfRange =
            new LykkeApiErrorCode(nameof(BCryptWorkFactorOutOfRange), "BCrypt work factor is out of range");

        public static readonly ILykkeApiErrorCode BCryptInternalError =
            new LykkeApiErrorCode(nameof(BCryptInternalError), "BCrypt internal error");

        public static readonly ILykkeApiErrorCode CountryFromRestrictedList =
            new LykkeApiErrorCode(nameof(CountryFromRestrictedList),
                "The residents from the country are not allowed to register");

        public static readonly ILykkeApiErrorCode CountryCodeInvalid =
            new LykkeApiErrorCode(nameof(CountryCodeInvalid), "The country code is invalid.");

        public static readonly ILykkeApiErrorCode InvalidPhoneFormat =
            new LykkeApiErrorCode(nameof(InvalidPhoneFormat), "Invalid phone number format");

        public static readonly ILykkeApiErrorCode PhoneNumberInUse =
            new LykkeApiErrorCode(nameof(PhoneNumberInUse), "Phone number already registered");
    }
}
