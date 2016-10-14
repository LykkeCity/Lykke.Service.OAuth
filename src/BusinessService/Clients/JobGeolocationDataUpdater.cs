using System.Collections.Concurrent;
using System.Threading.Tasks;
using BusinessService.Infrastructure;
using Core.AuditLog;
using Core.Clients;
using Core.Country;

namespace BusinessService.Clients
{
    public class JobGeolocationDataUpdater : IRegistrationConsumer, IApplicationService
    {
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly IIpGeoLocationService _ipGeoLocationService;
        private readonly object _lockObject = new object();
        private readonly IPersonalDataRepository _personalDataRepository;

        private readonly ConcurrentQueue<RegistrationEvent> _queue = new ConcurrentQueue<RegistrationEvent>();

        public JobGeolocationDataUpdater(
            IPersonalDataRepository personalDataRepository, IAuditLogRepository auditLogRepository,
            IIpGeoLocationService ipGeoLocationService)
        {
            _personalDataRepository = personalDataRepository;
            _auditLogRepository = auditLogRepository;
            _ipGeoLocationService = ipGeoLocationService;
        }

        public void ConsumeRegistration(IClientAccount account, string ip, string language)
        {
            lock (_lockObject)
            {
                _queue.Enqueue(new RegistrationEvent
                {
                    ClientAccount = account,
                    Ip = ip,
                    Language = language
                });
            }

            Task.Run(async () => await Execute());
        }


        public RegistrationEvent GetEvent()
        {
            lock (_lockObject)
            {
                if (_queue.Count == 0)
                    return null;

                RegistrationEvent localValue;
                while (_queue.TryDequeue(out localValue))
                    return localValue;

                return null;
            }
        }

        private async Task Execute()
        {
            var evnt = GetEvent();

            while (evnt != null)
            {
                var geo = await _ipGeoLocationService.GetLocationDetailsByIpAsync(evnt.Ip, evnt.Language);
                var clientId = evnt.ClientAccount.Id;
                var dataBefore = await _personalDataRepository.GetAsync(clientId);
                await
                    _personalDataRepository.UpdateGeolocationDataAsync(clientId, geo.CountryCode, geo.City);
                var dataAfter = await _personalDataRepository.GetAsync(clientId);
                await
                    _auditLogRepository.AddAuditRecordAsync(clientId, dataBefore, dataAfter,
                        AuditRecordType.PersonalData, "JobGeolocationDataUpdater");

                evnt = GetEvent();
            }
        }

        public class RegistrationEvent
        {
            public IClientAccount ClientAccount { get; set; }
            public string Ip { get; set; }
            public string Language { get; set; }
        }
    }
}