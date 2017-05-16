using BusinessService.Clients;
using Common.Log;
using Core.AuditLog;
using Core.Clients;
using Core.Country;
using NSubstitute;
using Xunit;

namespace BusinessService.Tests.Clients
{
    public class RegistrationConsumerTests
    {
        [Fact]
        public void Does_RegistrationConsumer_Queue_Work()
        {
            // arrange

            // act
            var jobGeoLocationUpdate = CreateJobGeolocationDataUpdater();

            jobGeoLocationUpdate.ConsumeRegistration(Arg.Any<IClientAccount>(), Arg.Any<string>(), Arg.Any<string>());

            var registrationEvent = jobGeoLocationUpdate.GetEvent();
            var registrationEvent2 = jobGeoLocationUpdate.GetEvent();

            // assert
            Assert.NotNull(registrationEvent);
            Assert.Null(registrationEvent2);
        }

        private static JobGeolocationDataUpdater CreateJobGeolocationDataUpdater()
        {
            var srvGeoLocation = Substitute.For<IIpGeoLocationService>();
            var personalDataService = Substitute.For<IPersonalDataService>();
            var log = Substitute.For<ILog>();
            var auditLogRepository = Substitute.For<IAuditLogRepository>();
            var jobGeoLocationUpdate = new JobGeolocationDataUpdater(personalDataService,
                auditLogRepository, srvGeoLocation);
            return jobGeoLocationUpdate;
        }
    }
}