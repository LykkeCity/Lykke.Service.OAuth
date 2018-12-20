using System.Collections.Generic;
using System.Linq;
using Core.ExternalProvider;
using Core.ExternalProvider.Settings;

namespace Lykke.Service.OAuth.Services.ExternalProvider
{
    public class ExternalProvidersValidation : IExternalProvidersValidation
    {
        public bool RequirePhoneVerification { get; }

        public bool RequireEmailVerification { get; }

        private readonly HashSet<string> _validLykkeIdps;
        private readonly HashSet<string> _validExternalIdps;

        public ExternalProvidersValidation(ValidationSettings validationSettings)
        {
            RequirePhoneVerification = validationSettings.RequirePhoneVerification;

            RequireEmailVerification = validationSettings.RequireEmailVerification;

            _validLykkeIdps = validationSettings.ValidLykkeIdps.ToHashSet();

            _validExternalIdps = validationSettings.ValidExternalIdps.ToHashSet();
        }

        public bool IsValidLykkeIdp(string idp)
        {
            return !string.IsNullOrWhiteSpace(idp) && _validLykkeIdps.Contains(idp);
        }

        public bool IsValidExternalIdp(string idp)
        {
            return !string.IsNullOrWhiteSpace(idp) && _validExternalIdps.Contains(idp);
        }
    }
}
