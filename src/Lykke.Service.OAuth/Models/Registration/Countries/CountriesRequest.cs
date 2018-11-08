using System.ComponentModel.DataAnnotations;

namespace Lykke.Service.OAuth.Models.Registration.Countries
{
    /// <summary>
    ///     Request object for countries.
    /// </summary>
    public class CountriesRequest
    {
        /// <summary>
        ///     Registration id token.
        /// </summary>
        [Required]
        public string RegistrationId { get; set; }
    }
}
