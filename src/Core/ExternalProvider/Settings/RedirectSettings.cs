namespace Core.ExternalProvider.Settings
{
    public class RedirectSettings
    {
        /// <summary>
        ///     Acr values used during old lykke flow, passed to ironclad authentication.
        /// </summary>
        public string OldLykkeSignInIroncladAuthAcrValues { get; set; }

        /// <summary>
        ///     Minimum version of ios to replace 302 request with 200 html form.
        /// </summary>
        public int IosMinVersionForCustomRedirect { get; set; }
    }
}
