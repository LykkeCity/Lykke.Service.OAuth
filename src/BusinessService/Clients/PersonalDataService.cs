using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Extenstions;
using Common.Log;
using Core.Clients;
using Core.Settings;
using Flurl.Http;

namespace BusinessService.Clients
{
    public class PersonalDataService : IPersonalDataService
    {
        private readonly PersonalDataServiceSettings _settings;
        private readonly ILog _log;

        public PersonalDataService(PersonalDataServiceSettings settings, ILog log)
        {
            _settings = settings;
            _log = log;
        }

        public async Task<IPersonalData> GetAsync(string id)
        {
            return await GetDataAsync<PersonalData>(id);
        }

        public async Task<IFullPersonalData> GetFullAsync(string id)
        {
            return await GetDataAsync<FullPersonalData>($"full/{id}");
        }

        public async Task<IEnumerable<IPersonalData>> GetAsync(IEnumerable<string> id)
        {
            return await PostDataAsync<PersonalData[]>(id.ToArray(), "list");
        }

        public async Task<IEnumerable<IFullPersonalData>> GetFullAsync(IEnumerable<string> id)
        {
            return await PostDataAsync<FullPersonalData[]>(id.ToArray(), "full/list");
        }

        /// <summary>
        /// Find clients by email
        /// </summary>
        public async Task<IPersonalData> FindClientsByEmail(string email)
        {
            return await GetDataAsync<PersonalData>($"?email={email}");
        }

        /// <summary>
        /// Search client by part of full name, email or contact phone
        /// </summary>
        public async Task<IPersonalData> FindClientsByPhrase(string phrase)
        {
            return await GetDataAsync<PersonalData>($"search?phrase={phrase}");
        }

        public Task SaveAsync(IFullPersonalData personalData)
        {
            return PostDataAsync<object>(personalData, "");
        }

        public Task UpdateAsync(IPersonalData personalData)
        {
            return PutDataAsync(personalData, "");
        }

        public Task ChangeFullNameAsync(string id, string fullName)
        {
            return PutDataAsync(ChangeFieldRequest.Create(fullName), $"{id}/fullName");
        }

        public Task ChangeFirstNameAsync(string id, string firstName)
        {
            return PutDataAsync(ChangeFieldRequest.Create(firstName), $"{id}/firstName");
        }

        public Task ChangeLastNameAsync(string id, string lastName)
        {
            return PutDataAsync(ChangeFieldRequest.Create(lastName), $"{id}/lastName");
        }

        public Task ChangeCountryAsync(string id, string country)
        {
            return PutDataAsync(ChangeFieldRequest.Create(country), $"{id}/country");
        }

        public Task ChangeCityAsync(string id, string city)
        {
            return PutDataAsync(ChangeFieldRequest.Create(city), $"{id}/city");
        }

        public Task ChangeZipAsync(string id, string zip)
        {
            return PutDataAsync(ChangeFieldRequest.Create(zip), $"{id}/zip");
        }

        public Task ChangeAddressAsync(string id, string address)
        {
            return PutDataAsync(ChangeFieldRequest.Create(address), $"{id}/address");
        }

        public Task ChangeContactPhoneAsync(string id, string phoneNumber)
        {
            return PutDataAsync(ChangeFieldRequest.Create(phoneNumber), $"{id}/phoneNumber");
        }

        public Task UpdateGeolocationDataAsync(string id, string countryCode, string city)
        {
            return PutDataAsync(ChangeGeolocationRequest.Create(countryCode, city), $"{id}/geolocation");
        }

        public Task ChangePasswordHintAsync(string id, string newHint)
        {
            return PutDataAsync(ChangeFieldRequest.Create(newHint), $"{id}/passwordHint");
        }

        public Task SetReferralCodeAsync(string id, string refCode)
        {
            return PutDataAsync(ChangeFieldRequest.Create(refCode), $"{id}/refCode");
        }

        public Task ChangeSpotRegulatorAsync(string id, string spotRegulator)
        {
            return PutDataAsync(ChangeFieldRequest.Create(spotRegulator), $"{id}/spotRegulator");
        }

        public Task ChangeMarginRegulatorAsync(string id, string marginRegulator)
        {
            return PutDataAsync(ChangeFieldRequest.Create(marginRegulator), $"{id}/marginRegulator");
        }


        #region Helpers

        private IFlurlClient GetClient(string action)
        {
            return $"{_settings.ServiceUri}/api/PersonalData/{action}"
                .WithHeader("api-key", _settings.ApiKey);
        }

        private async Task<TResponse> GetDataAsync<TResponse>(string action)
        {
            try
            {
                return await GetClient(action)
                    .GetJsonAsync<TResponse>();
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(PersonalDataService), action, "GET", ex);
                throw;
            }
        }

        private async Task<TResponse> PostDataAsync<TResponse>(object request, string action)
        {
            try
            {
                return await GetClient(action)
                    .PostJsonAsync(request)
                    .ReceiveJson<TResponse>();
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(PersonalDataService), action, request.ToJson(), ex);
                throw;
            }
        }

        private async Task PutDataAsync(object request, string action)
        {
            try
            {
                await GetClient(action)
                    .PutJsonAsync(request);
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(PersonalDataService), action, request.ToJson(), ex);
                throw;
            }
        }


        #endregion

    }
}
