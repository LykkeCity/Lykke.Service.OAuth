using System.Threading.Tasks;

namespace Core.Messages
{
    public interface ISrvEmailsFacade
    {
        Task SendWelcomeEmail(string email, string clientId);
    }
}
