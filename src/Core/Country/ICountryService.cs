using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Country
{
    public interface ICountryService
    {
        Task<IEnumerable<CountryItem>> GetCountryListAsync(string language);
    }
}