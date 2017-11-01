using System.Threading.Tasks;

namespace Core.Log
{
    public interface IClientLog
    {
        Task WriteAsync(string userId, string dataId);
    }
}
