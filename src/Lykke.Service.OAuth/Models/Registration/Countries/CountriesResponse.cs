using System.Collections.Generic;
using System.Linq;
using Core.Countries;

namespace Lykke.Service.OAuth.Models.Registration.Countries
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
        /// <param name="userLocationCountry">User location country.</param>
        public CountriesResponse(
            IEnumerable<CountryInfo> countries,
            IEnumerable<CountryInfo> restrictedCountriesOfResidence,
            CountryInfo userLocationCountry)
        {
            Countries = countries.Select(info => new CountryModel(info));
            RestrictedCountriesOfResidence =
                restrictedCountriesOfResidence.Select(info => new RestrictedCountryOfResidenceModel(info));

            if (userLocationCountry != null) UserLocationCountry = new UserLocationCountryModel(userLocationCountry);
        }

        /// <summary>
        ///     List of all available countries
        /// </summary>
        public IEnumerable<CountryModel> Countries { get; }

        /// <summary>
        ///     List of iso2 country codes,
        ///     which are restricted as country of residence
        /// </summary>
        public IEnumerable<RestrictedCountryOfResidenceModel> RestrictedCountriesOfResidence { get; }

        /// <summary>
        ///     User location country
        /// </summary>
        public UserLocationCountryModel UserLocationCountry { get; }
    }
}
