using System;
using System.Threading.Tasks;
using BusinessService.Infrastructure;
using Core.Messages;
using Core.Messages.Email;
using Core.Messages.Email.MessagesData;

namespace BusinessService.Messages.Email
{
    public class SrvEmailsFacade : ISrvEmailsFacade, IApplicationService
    {
        private readonly ITransactionalEmailSender _emailSender;

        public SrvEmailsFacade(ITransactionalEmailSender emailSender)
        {
            _emailSender = emailSender;
        }

        public async Task SendWelcomeEmail(string email, string clientId)
        {
            var msgData = new RegistrationData
            {
                ClientId = clientId,
                Year = DateTime.UtcNow.Year.ToString()
            };

            await _emailSender.SendWelcomeEmailAsync(email, msgData);
        }
    }
}
