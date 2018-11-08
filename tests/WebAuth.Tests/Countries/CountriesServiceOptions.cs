using System.Collections.Generic;
using Lykke.Common;
using Lykke.Service.IpGeoLocation;
using NSubstitute;

namespace WebAuth.Tests.Countries
{
    /// <summary>
    ///     Util class for configuring test countries service.
    /// </summary>
    internal class CountriesServiceOptions
    {
        public IEnumerable<CountryItem> CountryItems { get; set; }
        public IEnumerable<string> RestrictedCountriesOfResidenceIso2 { get; set; }
        public IIpGeoLocationClient GeoLocationClient { get; set; }

        public CountriesServiceOptions()
        {
            CountryItems = new List<CountryItem>();
            RestrictedCountriesOfResidenceIso2 = new List<string>();
            GeoLocationClient = Substitute.For<IIpGeoLocationClient>();
        }
    }
}
