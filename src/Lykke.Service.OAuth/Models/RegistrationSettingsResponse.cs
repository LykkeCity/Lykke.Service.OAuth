namespace Lykke.Service.OAuth.Models
{
    /// <summary>
    /// Registration settings
    /// </summary>
    public class RegistrationSettingsResponse
    {
        /// <summary>
        /// Minimum bCrypt work factor value to be used for hashing
        /// </summary>
        public int BCryptWorkFactor { get; set; }
    }
}
