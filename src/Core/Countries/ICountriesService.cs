using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Exceptions;

namespace Core.Countries
{
    /// <summary>
    ///     Service for managing countries.
    /// </summary>
    public interface ICountriesService
    {
        /// <summary>
        ///     List of all available countries.
        /// </summary>
        IEnumerable<CountryInfo> Countries { get; }

        /// <summary>
        ///     List of iso2 country codes, that are restricted as country of residence during registration process.
        /// </summary>
        IEnumerable<CountryInfo> RestrictedCountriesOfResidence { get; }

        /// <summary>
        ///     Get country information by ip address.
        /// </summary>
        /// <param name="ip">Ip address.</param>
        /// <returns>Country to which ip address belongs.</returns>
        /// <exception cref="CountryNotFoundException">
        ///     Thrown when ip is invalid ip address.
        ///     Thrown when country could not be found by provided ip.
        ///     Thrown when country could not be found in countries list, by country code.
        /// </exception>
        Task<CountryInfo> GetCountryByIpAsync(string ip);

        /// <summary>
        /// Checks if country code iso2 is restricted
        /// </summary>
        /// <param name="countryCode">Country code in iso2 format</param>
        /// <returns>True, if country is restricted, false - otherwise</returns>
        /// <exception cref="ArgumentNullException">Thrown when country code is null or empty</exception>
        bool IsCodeIso2Restricted(string countryCode);

        /// <summary>
        /// Check if country code iso2 is a valid country
        /// </summary>
        /// <param name="countryCode">Country code in iso2 format</param>
        /// <returns>True, id country code iso2 is valid, false - otherwise</returns>
        /// <exception cref="ArgumentNullException">Thrown when country code is null or empty</exception>
        bool IsCodeIso2Valid(string countryCode);

        /// <summary>
        /// Validates country code iso2
        /// </summary>
        /// <param name="countryCode">Country code in iso2 format</param>
        /// <exception cref="ArgumentNullException">Thrown when country code is null or empty</exception>
        /// <exception cref="CountryFromRestrictedListException">Thrown when country is in restricted list</exception>
        /// <exception cref="CountryInvalidException">Thrown when country is not valid</exception>
        void ValidateCode(string countryCode);
    }
}
