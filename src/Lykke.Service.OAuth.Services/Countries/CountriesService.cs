using System.Collections.Generic;
using System.Linq;
using Core.Countries;
using Lykke.Common;

namespace Lykke.Service.OAuth.Services.Countries
{

    /// <inheritdoc />
    public class CountriesService : ICountriesService
    {
        public CountriesService(IEnumerable<string> restrictedCountriesOfResidenceIso2)
        {
            var common = new CountryPhoneCodes();
            Countries = common.GetCountries().Select(item => new CountryInfo(item));
            RestrictedCountriesOfResidenceIso2 = restrictedCountriesOfResidenceIso2;
        }

        /// <inheritdoc />
        public IEnumerable<CountryInfo> Countries { get; }

        /// <inheritdoc />
        public IEnumerable<string> RestrictedCountriesOfResidenceIso2 { get; }
    }
}
