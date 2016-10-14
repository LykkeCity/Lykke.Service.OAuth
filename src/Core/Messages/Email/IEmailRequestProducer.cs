using System.Threading.Tasks;
using Core.Broadcast;

namespace Core.Messages.Email
{
    public static class EmailRequest
    {
        public const string WelcomeEmail = "WelcomeEmail";
        public const string WelcomeFxEmail = "WelcomeFxEmail";
        public const string ConfirmationEmail = "ConfirmationEmail";
    }

    public interface IEmailRequestProducer
    {
        Task SendEmailAsync<T>(string emailAddress, T messageData);
        Task SendEmailBroadcastAsync<T>(BroadcastGroup broadcastGroup, T messageData);
    }
}
