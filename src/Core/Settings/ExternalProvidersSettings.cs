using Core.ExternalProvider;

namespace Core.Settings
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
    }
}
