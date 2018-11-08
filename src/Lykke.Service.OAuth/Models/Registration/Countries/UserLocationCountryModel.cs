using Core.Countries;

namespace Lykke.Service.OAuth.Models.Registration.Countries
{
    /// <summary>
    ///     User location country model
    /// </summary>
    public class UserLocationCountryModel
    {
        /// <summary>
        ///     Iso2 country code
        /// </summary>
        public string Iso2 { get; }

        public UserLocationCountryModel(CountryInfo countryInfo)
        {
            Iso2 = countryInfo.Iso2;
        }
    }
}
