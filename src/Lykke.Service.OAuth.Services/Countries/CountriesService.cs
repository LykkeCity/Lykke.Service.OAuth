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
            RestrictedCountriesOfResidence = Countries.Where(info => restrictedCountriesOfResidenceIso2.Contains(info.Iso2));
        }

        /// <inheritdoc />
        public IEnumerable<CountryInfo> Countries { get; }

        /// <inheritdoc />
        public IEnumerable<CountryInfo> RestrictedCountriesOfResidence { get; }
    }
}
