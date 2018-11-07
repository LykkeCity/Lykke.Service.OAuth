using System.Collections.Generic;

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
    }
}
