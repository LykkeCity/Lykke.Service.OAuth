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
        private readonly IEmailRequestProducer _emailRequestProducer;

        public SrvEmailsFacade(IEmailRequestProducer emailRequestProducer)
        {
            _emailRequestProducer = emailRequestProducer;
        }

        public async Task SendWelcomeEmail(string email, string clientId)
        {
            var msgData = new RegistrationData
            {
                ClientId = clientId,
                Year = DateTime.UtcNow.Year.ToString()
            };
            await _emailRequestProducer.SendEmailAsync(email, msgData);
        }
    }
}
