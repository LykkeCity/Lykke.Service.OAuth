using Lykke.Common;

namespace Core.Countries
{
    /// <summary>
    ///     Country informtion model.
    /// </summary>
    public class CountryInfo
    {
        /// <summary>
        ///     Iso3 country code.
        /// </summary>
        public string Iso3 { get; }

        /// <summary>
        ///     Iso2 country code.
        /// </summary>
        public string Iso2 { get; }

        /// <summary>
        ///     Country display name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     Phone prefix.
        /// </summary>
        public string PhonePrefix { get; }

        public CountryInfo(CountryItem countryItem)
        {
            Iso3 = countryItem.Id;
            Iso2 = countryItem.Iso2;
            Name = countryItem.Name;
            PhonePrefix = countryItem.Prefix;
        }
    }
}
