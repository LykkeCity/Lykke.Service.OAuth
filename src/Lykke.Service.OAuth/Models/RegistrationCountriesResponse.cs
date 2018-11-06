using System.Collections.Generic;
using System.Linq;
using Core.Countries;

namespace Lykke.Service.OAuth.Models
{
    /// <summary>
    ///     Countries data for registration process.
    /// </summary>
    public class RegistrationCountriesResponse
    {
        /// <summary>
        ///     Construct response, based on
        /// </summary>
        /// <param name="countries">All available counries.</param>
        /// <param name="restrictedCountriesOfResidence">List of restricted countries.</param>
        public RegistrationCountriesResponse(IEnumerable<CountryInfo> countries,
            IEnumerable<string> restrictedCountriesOfResidence)
        {
            Countries = countries.Select(info => new RegistrationCountryViewModel(info));
            RestrictedCountriesOfResidence =
                restrictedCountriesOfResidence;
        }

        public IEnumerable<RegistrationCountryViewModel> Countries { get; }

        /// <summary>
        ///     List of iso2 country codes.
        ///     Which are restricted as country of residence.
        /// </summary>
        public IEnumerable<string> RestrictedCountriesOfResidence { get; }
    }
}
