using System.Threading.Tasks;
using Common.Log;
using Core.Registration;
using Lykke.Common.Log;
using Lykke.Service.Registration.Contract.Events;

namespace Lykke.Service.OAuth.Services
{
    public class RegistrationFinishedProjection
    {
        private readonly IRegistrationRepository _registrationRepository;
        private readonly ILog _log;

        public RegistrationFinishedProjection(
            IRegistrationRepository registrationRepository, ILogFactory logFactory)
        {
            _registrationRepository = registrationRepository;
            _log = logFactory.CreateLog(this);
        }

        public async Task Handle(RegistrationFinishedEvent evt)
        {
            if (evt.RegistrationId == null)
            {
                _log.Error("Empty registration id.");
            }

            //todo @mkobzev: add command which will change the domain
            var result = await _registrationRepository.DeleteIfExistAsync(evt.RegistrationId);

            if (!result) _log.Error($"Can not delete registration {evt.RegistrationId}.");
        }
    }
}
