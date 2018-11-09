using Lykke.Service.OAuth.Attributes;

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
        [ValidateRegistrationId]
        public string RegistrationId { get; set; }
    }
}
