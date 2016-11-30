using System;
using System.Threading.Tasks;
using Core.Messages;
using Core.Messages.Email;
using Core.Messages.Email.MessagesData;

namespace BusinessService.Messages.Email
{
    public class TransactionalEmailSender : ITransactionalEmailSender
    {
        private readonly IEmailSender _emailSender;
        private readonly IEmailGenerator _emailGenerator;

        public TransactionalEmailSender(
            IEmailSender emailSender,
            IEmailGenerator emailGenerator)
        {
            _emailSender = emailSender;
            _emailGenerator = emailGenerator;
        }

        public async Task SendWelcomeEmailAsync(string email, RegistrationData model)
        {
            var msg = await _emailGenerator.GenerateWelcomeMsg(model);
            await _emailSender.SendEmailAsync(email, msg);
        }
    }
}
