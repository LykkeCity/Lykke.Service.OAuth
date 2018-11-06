namespace Lykke.Service.OAuth.Models.Registration
{
    /// <summary>
    /// Registration response
    /// </summary>
    public class RegistrationResponse
    {
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="registrationId"></param>
        public RegistrationResponse(string registrationId)
        {
            RegistrationId = registrationId;
        }

        /// <summary>
        /// Id. Should be used for progress tracking
        /// </summary>
        public string RegistrationId { get; }
    }
}
