using System.Threading.Tasks;
using BusinessService.Infrastructure;
using Core.Broadcast;
using Core.Messages.Email;

namespace BusinessService.Messages.Email
{
    public class EmailRequestProducer : IEmailRequestProducer, IApplicationService
    {
        private readonly IEmailCommandProducer _emailCommandProducer;

        public EmailRequestProducer(IEmailCommandProducer emailCommandProducer)
        {
            _emailCommandProducer = emailCommandProducer;
        }

        public async Task SendEmailAsync<T>(string email, T msgData)
        {
            await _emailCommandProducer.ProduceSendEmailCommand(email, msgData);
        }

        public async Task SendEmailBroadcastAsync<T>(BroadcastGroup broadcastGroup, T messageData)
        {
            await _emailCommandProducer.ProduceSendEmailBroadcast(broadcastGroup, messageData);
        }
    }
}
