using System.Collections.Generic;

namespace Lykke.Service.OAuth.Settings
{
    public class RegistrationProcessSettings
    {
        /// <summary>
        ///     List of iso2 country codes, that are restricted as country of residence during registration process.
        /// </summary>
        public IEnumerable<string> RestrictedCountriesOfResidenceIso2 { get; set; }
    }
}
