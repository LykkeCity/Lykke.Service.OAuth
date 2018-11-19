using System.Threading.Tasks;
using Core.Registration;
using Lykke.Service.Registration.Contract.Events;
using Microsoft.Extensions.Logging;

namespace Lykke.Service.OAuth.Services
{
    public class RegistrationFinishedProjection
    {
        private readonly IRegistrationRepository _registrationRepository;
        private readonly ILogger _logger;

        public RegistrationFinishedProjection(
            IRegistrationRepository registrationRepository, ILoggerFactory loggerFactory)
        {
            _registrationRepository = registrationRepository;
            _logger = loggerFactory.CreateLogger(GetType());
        }
        public async Task Handle(RegistrationFinishedEvent evt)
        {
            var result = await _registrationRepository.DeleteIfExistAsync(evt.RegistrationId);

            if (!result) _logger.LogError($"Can not delete registration {evt.RegistrationId}.");
        }
    }
}
