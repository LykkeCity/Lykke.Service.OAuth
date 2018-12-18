using System.Collections.Generic;
using Lykke.SettingsReader.Attributes;

namespace Core.ExternalProvider.Settings
{
    /// <summary>
    ///     External providers validation settings.
    /// </summary>
    public class ValidationSettings
    {
        /// <summary>
        ///     Valid identity provider names for lykke identity provider.
        ///     Default: ["lykke"]
        /// </summary>
        [Optional]
        public List<string> LykkeIdpValidNames { get; set; } = new List<string>
        {
            "lykke"
        };

        /// <summary>
        ///     Indicates if phone verification is required for external users.
        ///     Default: true.
        /// </summary>
        [Optional]
        public bool RequirePhoneVerification { get; set; } = true;

        /// <summary>
        ///     Indicates if email verification is required for external users.
        ///     Default: true.
        /// </summary>
        [Optional]
        public bool RequireEmailVerification { get; set; } = true;
    }
}
