using System.Threading.Tasks;
using Core.Broadcast;

namespace Core.Messages.Email
{
    public interface IEmailCommandProducer
    {
        Task ProduceSendEmailCommand<T>(string mailAddress, T msgData);
        Task ProduceSendEmailBroadcast<T>(BroadcastGroup broadcastGroup, T msgData);
    }
}
