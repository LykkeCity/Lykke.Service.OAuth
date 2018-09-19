using System;
using System.Threading.Tasks;
using Core.Email;
using Lykke.Messages.Email;
using Lykke.Messages.Email.MessageData;

namespace BusinessService.Email
{
    public class EmailFacadeService : IEmailFacadeService
    {
        private readonly IEmailSender _emailSender;

        public EmailFacadeService(IEmailSender emailSender)
        {
            _emailSender = emailSender;
        }

        public async Task SendVerifyCode(string email, string code, string url)
        {
            await _emailSender.SendEmailAsync("Lykke", email,
                new RegistrationEmailVerifyData {Code = code, Year = DateTime.UtcNow.Year.ToString(), Url = url});
        }
    }
}
