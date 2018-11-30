using System;
using Common;
using Core.Registration;
using Lykke.Service.Registration.Contract.Client.Models;

namespace Lykke.Service.OAuth.Factories
{
    public class RequestModelFactory : IRequestModelFactory
    {
        public SafeAccountRegistrationModel CreateForRegistrationService(RegistrationModel registrationModel, string ip, string userAgent)
        {
            return new SafeAccountRegistrationModel
            {
                Email = registrationModel.Email,
                ClientId = registrationModel.ClientId,
                CountryFromPOA = CountryManager.Iso2ToIso3(registrationModel.CountryOfResidenceIso2),
                FullName = registrationModel.FirstName + " " + registrationModel.LastName,
                FirstName = registrationModel.FirstName,
                LastName = registrationModel.LastName,
                ContactPhone = registrationModel.PhoneNumber,
                ClientInfo = null,
                Changer = null,
                PartnerId = null,
                Salt = registrationModel.Salt,
                Hash = registrationModel.Hash,
                Cid = null,
                CreatedAt = registrationModel.Started,
                Hint = null,
                IosVersion = null,
                Ttl = TimeSpan.FromDays(3),
                Ip = ip,
                UserAgent = userAgent,
                RegistrationId = registrationModel.RegistrationId
            };
        }
    }
}
