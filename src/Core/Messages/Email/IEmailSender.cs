using System.Threading.Tasks;
using Core.Broadcast;

namespace Core.Messages.Email
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string emailAddress, EmailMessage message, string sender = null);
        Task SendBroadcastAsync(BroadcastGroup broadcastGroup, EmailMessage message);
    }
}
