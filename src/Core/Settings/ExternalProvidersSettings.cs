using System.Collections.Generic;
using Core.ExternalProvider;

namespace Core.Settings
{
    /// <summary>
    ///     External providers settings section.
    /// </summary>
    public class ExternalProvidersSettings
    {
        /// <summary>
        ///     List of configuration for external providers.
        /// </summary>
        public List<ExternalIdentityProvider> ExternalIdentityProviders { get; set; }
    }
}
