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
        ///     Valid lykke identity provider names.
        ///     It should be only one valid idp name for lykke in production,
        ///     but for testing purposes there could be more than one.
        ///     Default: ["lykke"]
        /// </summary>
        [Optional]
        public List<string> ValidLykkeIdps { get; set; } = new List<string>
        {
            "lykke"
        };

        /// <summary>
        ///     Valid extrenal identity provider names.
        ///     Default: null
        /// </summary>
        public List<string> ValidExternalIdps { get; set; }

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
