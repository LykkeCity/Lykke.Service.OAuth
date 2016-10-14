using System.Threading.Tasks;

namespace Core.Country
{
    public interface IIpGeoLocationService
    {
        Task<IpGeoLocationData> GetLocationDetailsByIpAsync(string ip, string language);
    }
}