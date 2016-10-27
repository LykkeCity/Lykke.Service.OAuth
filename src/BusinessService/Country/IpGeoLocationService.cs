using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using BusinessService.Infrastructure;
using Core.Country;
using Core.Settings;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;

namespace BusinessService.Country
{
    public class IpGeoLocationService : IIpGeoLocationService, IApplicationService
    {
        private readonly IBaseSettings _baseSettings;

        public IpGeoLocationService(IBaseSettings baseSettings)
        {
            _baseSettings = baseSettings;
        }

        public async Task<IpGeoLocationData> GetLocationDetailsByIpAsync(string ip, string language)
        {
            var webApiServerUri = new UriBuilder($"{_baseSettings.LykkeServiceApi.ServiceUri}/api/ipgeolocation");

            var queryStrings = new Dictionary<string, string>
            {
                {"ip", ip},
                {"language", language}
            };

            var requestUrl = QueryHelpers.AddQueryString(webApiServerUri.Uri.ToString(), queryStrings);

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plain"));

                var response = await client.GetAsync(requestUrl);

                if ((int) response.StatusCode == 201)
                {
                    return null;
                }

                var receiveStream = await response.Content.ReadAsStreamAsync();

                if (receiveStream == null)
                {
                    throw new Exception("ReceiveStream == null");
                }

                var serializer = new JsonSerializer();

                using (var sr = new StreamReader(receiveStream))
                {
                    using (var jsonTextReader = new JsonTextReader(sr))
                    {
                        return (IpGeoLocationData) serializer.Deserialize(jsonTextReader, typeof(IpGeoLocationData));
                    }
                }
            }
        }
    }
}