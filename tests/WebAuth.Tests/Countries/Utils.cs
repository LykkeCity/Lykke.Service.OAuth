using System;
using Lykke.Service.OAuth.Services.Countries;

namespace WebAuth.Tests.Countries
{
    /// <summary>
    ///     Utils class for countries service.
    /// </summary>
    internal static class Utils
    {
        internal static CountriesService CreateCountriesService(Action<CountriesServiceOptions> options = null)
        {
            var countriesServiceOptions = new CountriesServiceOptions();

            options?.Invoke(countriesServiceOptions);

            return new CountriesService(countriesServiceOptions.CountryItems,
                countriesServiceOptions.RestrictedCountriesOfResidenceIso2,
                countriesServiceOptions.GeoLocationClient);
        }
    }
}
