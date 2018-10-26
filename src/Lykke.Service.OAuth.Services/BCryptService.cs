using Core.Exceptions;
using Core.Extensions;
using Core.Services;

namespace Lykke.Service.OAuth.Services
{
    public class BCryptService : IBCryptService
    {
        private readonly int _bCryptWorkFactorSettings;

        public BCryptService(int bCryptWorkFactorSettings)
        {
            _bCryptWorkFactorSettings = bCryptWorkFactorSettings;
        }

        public void Verify(string source, string hash)
        {
            if (!BCrypt.Net.BCrypt.Verify(source, hash))
                throw new EmailHashInvalidException(source);

            int workFactor = hash.ExtractWorkFactor();

            if (workFactor != _bCryptWorkFactorSettings)
                throw new BCryptWorkFactorInvalidException(workFactor);
        }
    }
}
