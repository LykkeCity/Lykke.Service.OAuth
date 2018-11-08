using System.ComponentModel.DataAnnotations;

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
        [Required]
        public string RegistrationId { get; set; }
    }
}
