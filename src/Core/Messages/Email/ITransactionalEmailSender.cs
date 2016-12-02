using Core.Messages.Email.MessagesData;
using System.Threading.Tasks;

namespace Core.Messages.Email
{
    public interface ITransactionalEmailSender
    {
        Task SendWelcomeEmailAsync(string email, RegistrationData model);
    }
}
