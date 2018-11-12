using System;
using System.Linq;
using Common;
using Core.Countries;
using Core.Exceptions;

namespace Lykke.Service.OAuth.Services.Countries
{
    /// <summary>
    /// Extension methods for <see cref="ICountriesService"/>
    /// </summary>
    public static class CountriesServiceExtensions
    {
        /// <summary>
        /// Checks if country code iso2 is restricted
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

        /// <summary>
        /// Check if country code iso2 is a valid country
        /// </summary>
        /// <param name="service">Countries service</param>
        /// <param name="countryCode">Country code in iso2 format</param>
        /// <returns>True, id country code iso2 is valid, false - otherwise</returns>
        /// <exception cref="ArgumentNullException">Thrown when country code is null or empty</exception>
        public static bool IsCountryCodeIso2Valid(this ICountriesService service, string countryCode)
        {
            if (service == null)
                throw new ArgumentNullException(nameof(service));

            if (string.IsNullOrWhiteSpace(countryCode))
                throw new ArgumentNullException(nameof(countryCode));

            return CountryManager.GetCountryNameByIso2(countryCode) != string.Empty;
        }

        /// <summary>
        /// Validates country code iso2
        /// </summary>
        /// <param name="service">Countries service</param>
        /// <param name="countryCode">Country code in iso2 format</param>
        /// <exception cref="ArgumentNullException">Thrown when country code is null or empty</exception>
        /// <exception cref="CountryFromRestrictedListException">Thrown when country is in restricted list</exception>
        /// <exception cref="CountryInvalidException">Thrown when country is not valid</exception>
        public static void ValidateCountryCode(this ICountriesService service, string countryCode)
        {
            if (service == null)
                throw new ArgumentNullException(nameof(service));

            if (string.IsNullOrWhiteSpace(countryCode))
                throw new ArgumentNullException(nameof(countryCode));

            if (service.IsCountryCodeIso2Restricted(countryCode))
                throw new CountryFromRestrictedListException(countryCode);

            if (!service.IsCountryCodeIso2Valid(countryCode))
                throw new CountryInvalidException(countryCode);
        }
    }
}
