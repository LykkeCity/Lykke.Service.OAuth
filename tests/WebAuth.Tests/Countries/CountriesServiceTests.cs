using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Countries;
using FluentAssertions;
using Lykke.Common;
using Lykke.Service.IpGeoLocation;
using Lykke.Service.IpGeoLocation.Models;
using NSubstitute;
using Xunit;

namespace WebAuth.Tests.Countries
{
    public class CountriesServiceTests
    {
        private readonly IIpGeoLocationClient _fakeGeoLocationClient;
        private const string FakeIp = "123.123.123.123";

        public CountriesServiceTests()
        {
            _fakeGeoLocationClient = Substitute.For<IIpGeoLocationClient>();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void GetCountryByIp_IpIsNullOrWhitespace_ThrowsCountryNotFoundException(string ip)
        {
            // Arrange
            var countriesService = Utils.CreateCountriesService();

            // Act
            Func<Task> action = () => countriesService.GetCountryByIpAsync(ip);

            // Assert
            action.Should().Throw<CountryNotFoundException>()
                .WithMessage($"Country could not be found, Ip address is invalid: {ip}");
        }

        [Theory]
        [InlineData("asdasdasd")]
        [InlineData("1a.s2d.5f1a.0")]
        public void GetCountryByIp_IpIsNotValidIpAddress_ThrowsCountryNotFoundException(string ip)
        {
            // Arrange
            var countriesService = Utils.CreateCountriesService();

            // Act
            Func<Task> action = () => countriesService.GetCountryByIpAsync(ip);

            // Assert
            action.Should().Throw<CountryNotFoundException>()
                .And.Message.Should().Contain($"Ip address is invalid: {ip}");
        }

        [Fact]
        public void GetCountryByIp_CountryNotFoundByGeolocationClient_ThrowsCountryNotFoundException()
        {
            // Arrange
            _fakeGeoLocationClient.GetAsync(FakeIp).Returns(info => (IpGeolocationData) null);
            var countriesService = Utils.CreateCountriesService(options =>
            {
                options.GeoLocationClient = _fakeGeoLocationClient;
            });

            // Act
            Func<Task> action = () => countriesService.GetCountryByIpAsync(FakeIp);

            // Assert
            action.Should().Throw<CountryNotFoundException>()
                .And.Message.Should().Contain($"Country could not be found by ip: {FakeIp}");
            _fakeGeoLocationClient.Received(1).GetAsync(FakeIp);
        }

        [Fact]
        public void GetCountryByIp_CountryNotFoundInListOfAllCountries_ThrowsCountryNotFoundException()
        {
            // Arrange
            var fakeResponse = new IpGeolocationData
            {
                CountryCode = "CHN",
                Region = "Beijing",
                City = "Beijing",
                Isp = "China Unicom Beijing"
            };

            _fakeGeoLocationClient.GetAsync(FakeIp).Returns(info => fakeResponse);

            var countriesService = Utils.CreateCountriesService(options =>
            {
                options.GeoLocationClient = _fakeGeoLocationClient;
            });

            // Act
            Func<Task> action = () => countriesService.GetCountryByIpAsync(FakeIp);

            // Assert
            action.Should().Throw<CountryNotFoundException>()
                .And.Message.Should()
                .Contain($"Country could not be found for iso3 code: {fakeResponse.CountryCode}, ip:{FakeIp}");
            _fakeGeoLocationClient.Received(1).GetAsync(FakeIp);
        }

        [Fact]
        public async Task GetCountryByIp_CountryFoundInListOfAllCountries_ReturnsFoundCountry()
        {
            // Arrange
            var fakeCountryItem = new CountryItem("CHN", "+86");

            var fakeCountiresList = new List<CountryItem> {fakeCountryItem};

            var fakeResponse = new IpGeolocationData
            {
                CountryCode = "CHN",
                Region = "Beijing",
                City = "Beijing",
                Isp = "China Unicom Beijing"
            };

            var expectedResult = new CountryInfo
            {
                Iso3 = fakeCountryItem.Id,
                Iso2 = fakeCountryItem.Iso2,
                Name = fakeCountryItem.Name,
                PhonePrefix = fakeCountryItem.Prefix
            };

            _fakeGeoLocationClient.GetAsync(FakeIp).Returns(info => fakeResponse);

            var countriesService = Utils.CreateCountriesService(options =>
            {
                options.CountryItems = fakeCountiresList;
                options.GeoLocationClient = _fakeGeoLocationClient;
            });

            // Act
           var actualResult =  await countriesService.GetCountryByIpAsync(FakeIp);

            // Assert
            await _fakeGeoLocationClient.Received(1).GetAsync(FakeIp);
            actualResult.Should().BeEquivalentTo(expectedResult);
        }
    }
}
