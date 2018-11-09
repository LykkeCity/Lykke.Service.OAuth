using Lykke.Service.OAuth.Attributes;

namespace Lykke.Service.OAuth.Models.Registration
{
    /// <summary>
    ///     Request object for registration status.
    /// </summary>
    public class RegistrationStatusRequest
    {
        /// <summary>
        ///     Registration id token.
        /// </summary>
        [ValidateRegistrationId]
        public string RegistrationId { get; set; }
    }
}
