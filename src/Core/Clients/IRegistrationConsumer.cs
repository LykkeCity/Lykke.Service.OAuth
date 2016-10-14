using System.Threading.Tasks;

namespace Core.Clients
{
    public interface IRegistrationConsumer
    {
        void ConsumeRegistration(IClientAccount account, string ip, string language);
    }
}