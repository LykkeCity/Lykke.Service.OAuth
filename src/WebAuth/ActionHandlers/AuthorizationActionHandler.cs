using System.Linq;
using System.Threading.Tasks;
using Core.Clients;

namespace WebAuth.ActionHandlers
{
    public class AuthorizationActionHandler
    {
        private readonly IClientSettingsRepository _clientSettingsRepository;

        public AuthorizationActionHandler(IClientSettingsRepository clientSettingsRepository)
        {
            _clientSettingsRepository = clientSettingsRepository;
        }

        public Task<bool> IsTrustedApplicationAsync(string clientId, string applicatinoId)
        {
            return Task.FromResult(true);

            //currently we assume that all users trust to our platforms
//            var userTrustedApplications = await _clientSettingsRepository.GetSettings<ApplicationsSettings>(clientId);
//            var isTrustedApplication = userTrustedApplications.TrustedApplicationIds.Contains(applicatinoId);
//
//            return isTrustedApplication;
        }

        public async Task AddTrustedApplication(string clientId, string applicatinoId)
        {
            var userTrustedApplications = await _clientSettingsRepository.GetSettings<ApplicationsSettings>(clientId);
            var isTrustedApplication = userTrustedApplications.TrustedApplicationIds.Contains(applicatinoId);

            if (!isTrustedApplication)
            {
                var userTrustedApplicationsList = userTrustedApplications.TrustedApplicationIds.ToList();
                userTrustedApplicationsList.Add(applicatinoId);

                userTrustedApplications.TrustedApplicationIds = userTrustedApplicationsList.ToArray();

                await _clientSettingsRepository.SetSettings(clientId, userTrustedApplications);
            }
        }
    }
}