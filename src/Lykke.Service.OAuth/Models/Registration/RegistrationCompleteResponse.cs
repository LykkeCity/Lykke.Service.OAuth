namespace Lykke.Service.OAuth.Models.Registration
{
    /// <summary>
    /// Registration response
    /// </summary>
    public class RegistrationCompleteResponse
    {
        public RegistrationCompleteResponse(string authorizationToken, string notificationsId)
        {
            AuthorizationToken = authorizationToken;
            NotificationsId = notificationsId;
        }

        /// <summary>
        /// Authorization token. Should used in order to subscribe on push notifications
        /// </summary>
        public string NotificationsId { get; set; }

        /// <summary>
        /// Authorization token. Should be sent as bearer in order to get access to API
        /// </summary>
        public string AuthorizationToken { get; }
    }
}
