using System.Threading.Tasks;

namespace Core.Messages.Email
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, EmailMessage emailMessage, string sender = null);
    }
}
