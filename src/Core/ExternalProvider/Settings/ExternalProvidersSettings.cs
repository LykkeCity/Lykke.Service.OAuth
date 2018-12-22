using Lykke.SettingsReader.Attributes;

namespace Core.ExternalProvider.Settings
{
    /// <summary>
    ///     External providers settings section.
    /// </summary>
    public class ExternalProvidersSettings
    {
        /// <summary>
        ///     Ironclad client for user authentication.
        /// </summary>
        public IdentityProviderSettings IroncladAuth { get; set; }

        /// <summary>
        ///     Ironclad client for ironclad api.
        /// </summary>
        public IdentityProviderSettings IroncladApi { get; set; }

        /// <summary>
        /// External provider validation settings.
        /// </summary>
        [Optional]
        public ValidationSettings ValidationSettings { get; set; }

        //TODO:@gafanasiev Add summary.
        [Optional]
        public RedirectSettings RedirectSettings { get; set; }

    }
}
