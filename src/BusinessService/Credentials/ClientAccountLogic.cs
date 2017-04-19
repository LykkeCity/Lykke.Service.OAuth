using Common.PasswordTools;
using Core.Clients;
using Core.Partner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BusinessService.Credentials
{
    public class ClientAccountLogic
    {
        private readonly IClientAccountsRepository _clientAccountsRepository;
        private readonly IPartnerAccountPolicyRepository _partnerAccountPolicyRepository;

        public ClientAccountLogic(IClientAccountsRepository clientAccountsRepository,
            IPartnerAccountPolicyRepository partnerAccountPolicyRepository)
        {
            _clientAccountsRepository = clientAccountsRepository;
            _partnerAccountPolicyRepository = partnerAccountPolicyRepository;
        }

        public async Task<IClientAccount> AuthenticateUser(string email, string password, string partnerPublicId = null)
        {
            //Here we substitute publicId according to partner client account settings
            string publicId = await GetPartnerIdAccordingToSettings(partnerPublicId);
            IClientAccount client = await _clientAccountsRepository.AuthenticateAsync(email, password, publicId);

            if (client == null)
            {
                return null;
            }

            return client;
        }

        public async Task<IClientAccount> RegisterPartnerClientAccount(IClientAccount clientAccount, string password, string partnerPublicId = null)
        {
            string publicId = await GetPartnerIdAccordingToSettings(partnerPublicId);
            IClientAccount client = await _clientAccountsRepository.AuthenticateAsync(clientAccount.Email, password, publicId);

            if (client == null)
            {
                return null;
            }

            return client;
        }

        public async Task<bool> IsTraderWithEmailExistsForPartnerAsync(string email, string partnerId = null)
        {
            string partnerIdAccordingToPolicy = await GetPartnerIdAccordingToSettings(partnerId);
            IClientAccount client = await _clientAccountsRepository.GetByEmailAndPartnerIdAsync(email, partnerIdAccordingToPolicy);

            return client != null;
        }

        #region PrivateMethods

        /// <summary>
        /// Method returns true if we use different from LykkeWallet credentials else returns false
        /// </summary>
        public async Task<bool> UsePartnerCredentials(string partnerPublicId)
        {
            bool usePartnerCredentials = false;
            if (!string.IsNullOrEmpty(partnerPublicId))
            {
                IPartnerAccountPolicy policy = await _partnerAccountPolicyRepository.GetAsync(partnerPublicId);
                usePartnerCredentials = policy?.UseDifferentCredentials ?? false;
            }

            return usePartnerCredentials;
        }

        private async Task<string> GetPartnerIdAccordingToSettings(string partnerPublicId)
        {
            bool usePartnerCredentials = await UsePartnerCredentials(partnerPublicId);
            //Depends on partner settings
            string publicId = !usePartnerCredentials ? null : partnerPublicId;

            return publicId;
        }

        #endregion PrivateMethods
    }
}
