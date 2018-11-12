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
        public string Iso3 { get; set; }

        /// <summary>
        ///     Iso2 country code.
        /// </summary>
        public string Iso2 { get; set; }

        /// <summary>
        ///     Country display name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Phone prefix.
        /// </summary>
        public string PhonePrefix { get; set; }
    }
}
