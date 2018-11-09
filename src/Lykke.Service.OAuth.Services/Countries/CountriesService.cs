using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Countries;
using Lykke.Common;
using Lykke.Service.IpGeoLocation;

namespace Lykke.Service.OAuth.Services.Countries
{
    /// <inheritdoc />
    public class CountriesService : ICountriesService
    {
        private readonly IIpGeoLocationClient _geoLocationClient;

        public CountriesService(
            IEnumerable<CountryItem> countryItems,
            IEnumerable<string> restrictedCountriesOfResidenceIso2,
            IIpGeoLocationClient geoLocationClient)
        {
            _geoLocationClient = geoLocationClient;
            Countries = countryItems.Select(item => new CountryInfo
            {
                Iso2 = item.Iso2,
                Iso3 = item.Id,
                Name = item.Name,
                PhonePrefix = item.Prefix
            });

            RestrictedCountriesOfResidence =
                Countries.Where(info => restrictedCountriesOfResidenceIso2.Contains(info.Iso2));
        }

        /// <inheritdoc />
        public IEnumerable<CountryInfo> Countries { get; }

        /// <inheritdoc />
        public IEnumerable<CountryInfo> RestrictedCountriesOfResidence { get; }

        /// <inheritdoc />
        public async Task<CountryInfo> GetCountryByIpAsync(string ip)
        {
            if (string.IsNullOrWhiteSpace(ip)
                || (Uri.CheckHostName(ip) != UriHostNameType.IPv4 && Uri.CheckHostName(ip) != UriHostNameType.IPv6))
                throw new CountryNotFoundException($"Country could not be found, Ip address is invalid: {ip}");

            var geolocationData = await _geoLocationClient.GetAsync(ip);

            if (geolocationData == null)
                throw new CountryNotFoundException($"Country could not be found by ip: {ip}");

            var countryInfo = Countries.FirstOrDefault(info => info.Iso3 == geolocationData.CountryCode);

            if (countryInfo == null)
                throw new CountryNotFoundException($"Country could not be found for iso3 code: {geolocationData.CountryCode}, ip:{ip}");

            return countryInfo;
        }
    }
}
