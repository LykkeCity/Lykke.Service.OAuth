using System.Collections.Generic;
using Core.PasswordValidation;
using Lykke.Common.ApiLibrary.Contract;

namespace Lykke.Service.OAuth.ApiErrorCodes
{
    /// <summary>
    ///     Class for OAuth-related errors.
    /// </summary>
    public static class OAuthErrorCodes
    {
        /// <summary>
        /// Client not found
        /// </summary>
        public static ILykkeApiErrorCode ClientNotFound = new LykkeApiErrorCode(nameof(ClientNotFound), "Client not found.");
    }
}
