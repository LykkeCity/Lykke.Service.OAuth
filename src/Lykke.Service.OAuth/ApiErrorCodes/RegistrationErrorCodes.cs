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
    }
}
