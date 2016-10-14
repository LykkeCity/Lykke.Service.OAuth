using System.Threading.Tasks;
using AzureStorage.Queue;
using Core.Broadcast;
using Core.Messages.Email;
using Core.Messages.Email.MessagesData;

namespace AzureDataAccess.Email
{
    public class EmailCommandProducer : IEmailCommandProducer
    {
        private readonly IQueueExt _queueExt;

        public EmailCommandProducer(IQueueExt queueExt)
        {
            _queueExt = queueExt;

            _queueExt.RegisterTypes(
                QueueType.Create(EmailRequest.WelcomeEmail, typeof(QueueRequestModel<SendEmailData<RegistrationData>>))
            );
            _queueExt.RegisterTypes(
                QueueType.Create(EmailRequest.ConfirmationEmail,
                    typeof(QueueRequestModel<SendEmailData<EmailComfirmationData>>))
            );
        }

        public Task ProduceSendEmailCommand<T>(string mailAddress, T msgData)
        {
            var data = SendEmailData<T>.Create(mailAddress, msgData);
            var msg = new QueueRequestModel<SendEmailData<T>> {Data = data};
            return _queueExt.PutMessageAsync(msg);
        }

        public Task ProduceSendEmailBroadcast<T>(BroadcastGroup broadcastGroup, T msgData)
        {
            var data = SendBroadcastData<T>.Create(broadcastGroup, msgData);
            var msg = new QueueRequestModel<SendBroadcastData<T>> {Data = data};
            return _queueExt.PutMessageAsync(msg);
        }
    }
}