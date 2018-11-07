using System;
using System.Threading.Tasks;
using Core.Registration;
using Core.Services;
using Lykke.Service.ClientAccount.Client;

namespace Lykke.Service.OAuth.Services
{
    /// <inheritdoc />
    public class EmailValidationService : IEmailValidationService
    {
        private readonly IClientAccountClient _clientAccountClient;
        private readonly IBCryptService _bCryptService;
        private readonly IRegistrationRepository _registrationRepository;

        public EmailValidationService(
            IClientAccountClient clientAccountClient, 
            IBCryptService bCryptService,
            IRegistrationRepository registrationRepository)
        {
            _registrationRepository = registrationRepository;
            _clientAccountClient = clientAccountClient;
            _bCryptService = bCryptService;
        }
        
        /// <inheritdoc />
        public async Task<bool> IsEmailTakenAsync(string email, string hash)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentNullException(nameof(email));

            if (string.IsNullOrWhiteSpace(hash))
                throw new ArgumentNullException(nameof(hash));

           _bCryptService.Verify(email, hash);

            var userModel = await _registrationRepository.TryGetByEmailAsync(email);
            if (userModel != null && !userModel.CanEmailBeUsed()) return true;

            var accountExistsModel = await _clientAccountClient.IsTraderWithEmailExistsAsync(email, null);

            return accountExistsModel.IsClientAccountExisting;
        }
    }
}
