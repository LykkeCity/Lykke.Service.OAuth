namespace WebAuth.Models
{
    /// <summary>
    /// Registration responce
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
        /// Id. Can be used for progress tracking
        /// </summary>
        public string RegistrationId { get; }
    }
}
