using System.Collections.Generic;
using System.Linq;
using Core.Countries;
using Lykke.Service.OAuth.Models.Registration;

namespace Lykke.Service.OAuth.Models
{
    /// <summary>
    ///     Countries data for registration process
    /// </summary>
    public class CountriesResponse
    {
        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="countries">All available counries.</param>
        /// <param name="restrictedCountriesOfResidence">List of restricted countries.</param>
        public CountriesResponse(
            IEnumerable<CountryInfo> countries,
            IEnumerable<CountryInfo> restrictedCountriesOfResidence)
        {
            Countries = countries.Select(info => new Registration.CountryModel(info));
            RestrictedCountriesOfResidence = restrictedCountriesOfResidence.Select(info => new RestrictedCountryOfResidenceModel(info));
        }

        /// <summary>
        ///     List of all available countries
        /// </summary>
        public IEnumerable<Registration.CountryModel> Countries { get; }

        /// <summary>
        ///     List of iso2 country codes,
        ///     which are restricted as country of residence
        /// </summary>
        public IEnumerable<RestrictedCountryOfResidenceModel> RestrictedCountriesOfResidence { get; }
    }
}
