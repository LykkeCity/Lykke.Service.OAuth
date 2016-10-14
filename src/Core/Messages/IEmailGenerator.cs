using System.Threading.Tasks;
using Core.Clients;
using Core.Messages.Email;
using Core.Messages.Email.MessagesData;

namespace Core.Messages
{
    public interface IEmailGenerator
    {
        Task<EmailMessage> GenerateWelcomeMsg(RegistrationData kycOkData);
        Task<EmailMessage> GenerateConfirmEmailMsg(EmailComfirmationData registrationData);
        Task<EmailMessage> GenerateUserRegisteredMsg(IPersonalData messageData);
    }
}
