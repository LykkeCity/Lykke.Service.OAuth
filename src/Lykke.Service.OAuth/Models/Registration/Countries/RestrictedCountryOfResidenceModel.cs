using Core.Countries;

namespace Lykke.Service.OAuth.Models.Registration.Countries
{
    /// <summary>
    ///     Restricted country of residence during registration process
    /// </summary>
    public class RestrictedCountryOfResidenceModel
    {
        /// <summary>
        ///     Iso2 country code
        /// </summary>
        public string Iso2 { get; }

        public RestrictedCountryOfResidenceModel(CountryInfo countryInfo)
        {
            Iso2 = countryInfo.Iso2;
        }
    }
}
