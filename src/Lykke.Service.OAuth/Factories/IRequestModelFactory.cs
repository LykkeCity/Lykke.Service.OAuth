using Core.Registration;
using Lykke.Service.Registration.Contract.Client.Models;
namespace Lykke.Service.OAuth.Factories
{
    public interface IRequestModelFactory
    {
        SafeAccountRegistrationModel CreateForRegistrationService(RegistrationModel registrationModel, string ip, string userAgent, string referrer = null, string traffic = null);
    }
}
