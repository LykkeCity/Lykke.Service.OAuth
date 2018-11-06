using System.Collections.Generic;
using System.Linq;
using Core.Countries;

namespace Lykke.Service.OAuth.Models
{
    /// <summary>
    ///     Countries data for registration process
    /// </summary>
    public class RegistrationCountriesResponse
    {
        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="countries">All available counries.</param>
        /// <param name="restrictedCountriesOfResidence">List of restricted countries.</param>
        public RegistrationCountriesResponse(
            IEnumerable<CountryInfo> countries,
            IEnumerable<CountryInfo> restrictedCountriesOfResidence)
        {
            Countries = countries.Select(info => new RegistrationCountryModel(info));
            RestrictedCountriesOfResidence = restrictedCountriesOfResidence.Select(info => info.Iso2);
        }

        /// <summary>
        ///     List of all available countries
        /// </summary>
        public IEnumerable<RegistrationCountryModel> Countries { get; }

        /// <summary>
        ///     List of iso2 country codes,
        ///     which are restricted as country of residence
        /// </summary>
        public IEnumerable<string> RestrictedCountriesOfResidence { get; }
    }
}
