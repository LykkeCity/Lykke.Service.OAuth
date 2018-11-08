using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Countries
{
    /// <summary>
    ///     Service for managing countries.
    /// </summary>
    public interface ICountriesService
    {
        /// <summary>
        ///     List of all available countires.
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
    }
}
