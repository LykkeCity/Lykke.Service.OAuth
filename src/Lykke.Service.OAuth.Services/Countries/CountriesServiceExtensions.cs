using System;
using System.Linq;
using Core.Countries;

namespace Lykke.Service.OAuth.Services.Countries
{
    /// <summary>
    /// Extension methods for <see cref="ICountriesService"/>
    /// </summary>
    public static class CountriesServiceExtensions
    {
        /// <summary>
        /// Checks if country is restricted
        /// </summary>
        /// <param name="service">Countries service</param>
        /// <param name="countryCode">Country code in iso2 format</param>
        /// <returns>True, if country is restricted, false - otherwise</returns>
        /// <exception cref="ArgumentNullException">Thrown when country code is null or empty</exception>
        public static bool IsCountryCodeIso2Restricted(this ICountriesService service, string countryCode)
        {
            if (service == null)
                throw new ArgumentNullException(nameof(service));

            if (string.IsNullOrWhiteSpace(countryCode))
                throw new ArgumentNullException(nameof(countryCode));

            return service.RestrictedCountriesOfResidence.Any(x => countryCode.Equals(x.Iso2));
        }
    }
}
