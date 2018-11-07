using System.Linq;
using System.Threading.Tasks;
using Common;
using Core.Countries;
using Core.Exceptions;
using Core.Registration;
using Core.Services;

namespace Lykke.Service.OAuth.Services
{
    /// <inheritdoc />
    public class AccountInfoStepHandler : IAccountInfoStepHandler
    {
        private readonly ICountriesService _countriesService;
        private readonly IRegistrationRepository _registrationRepository;

        public AccountInfoStepHandler(
            ICountriesService countriesService, 
            IRegistrationRepository registrationRepository)
        {
            _countriesService = countriesService;
            _registrationRepository = registrationRepository;
        }

        /// <inheritedoc />
        public async Task HandleAsync(AccountInfoDto model)
        {
            RegistrationModel registrationModel = await _registrationRepository.GetByIdAsync(model.RegistrationId);

            if (_countriesService.RestrictedCountriesOfResidence.Any(x => model.CountryCodeIso2.Equals(x.Iso2)))
                throw new CountryFromRestrictedListException(model.CountryCodeIso2);

            var phoneNumberE164 = model.PhoneNumber.PreparePhoneNum().ToE164Number();

            if (phoneNumberE164 == null)
                throw new InvalidPhoneNumberFormatException(model.PhoneNumber);

            //todo: validate if phone number is already in use

            //todo: store step 2 information and update registration step
        }
    }
}
