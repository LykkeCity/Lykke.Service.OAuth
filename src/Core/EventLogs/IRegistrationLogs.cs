using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.EventLogs
{
    public interface IRegistrationLogEvent
    {
        string Id { get; }
        DateTime DateTime { get; }
        string ClientId { get; }

        string Email { get; }
        string FullName { get; }
        string ContactPhone { get; }

        /// <summary>
        /// Client software info
        /// </summary>
        string DeviceInfo { get; }
        string Country { get; }
        string City { get; }
        string Ip { get; }
        string Isp { get; }
    }

    public class RegistrationLogEvent : IRegistrationLogEvent
    {
        public DateTime DateTime { get; set; }
        public string ClientId { get; set; }
        public string Id { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string ContactPhone { get; set; }
        public string DeviceInfo { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public string Ip { get; set; }
        public string Isp { get; set; }


        public static RegistrationLogEvent Create(string clientId, string email, string fullname, string contactPhone, string deviceInfo, string ip, DateTime? dateTime = null)
        {
            return new RegistrationLogEvent
            {
                DateTime = dateTime ?? DateTime.UtcNow,
                DeviceInfo = deviceInfo,
                ClientId = clientId,
                FullName = fullname,
                Ip = ip,
                Email = email,
                ContactPhone = contactPhone
            };
        }
    }

    public interface IRegistrationLogs
    {
        Task<IRegistrationLogEvent> RegisterEventAsync(IRegistrationLogEvent evnt);

        Task<IEnumerable<IRegistrationLogEvent>> GetAsync(DateTime from, DateTime to);
        Task UpdateGeolocationDataAsync(string id, string countryCode, string city, string isp);
    }
}
