using Lykke.Common.ApiLibrary.Contract;

namespace Lykke.Service.OAuth.ApiErrorCodes
{
    /// <summary>
    ///     Class for storing common error codes that may happen in API.
    /// </summary>
    public static class LykkeApiCommonErrorCodes
    {
        /// <summary>
        ///     One of the provided values was not valid.
        /// </summary>
        public static readonly ILykkeApiErrorCode ModelValidationFailed =
            new LykkeApiErrorCode(nameof(ModelValidationFailed), "One of the provided values was not valid.");
    }
}
