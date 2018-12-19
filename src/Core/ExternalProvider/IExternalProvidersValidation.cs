namespace Core.ExternalProvider
{
    public interface IExternalProvidersValidation
    {
        bool IsValidLykkeIdp(string idp);

        bool IsValidExternalIdp(string idp);

        bool RequirePhoneVerification { get; }

        bool RequireEmailVerification { get; }
    }
}
