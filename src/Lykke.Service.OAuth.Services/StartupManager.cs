using Common.Log;
using Core.Exceptions;
using Core.Services;
using JetBrains.Annotations;
using Lykke.Common.Log;

namespace Lykke.Service.OAuth.Services
{
    public class StartupManager : IStartupManager
    {
        private readonly int _bCryptWorkFactor;
        private readonly ILog _log;
        private const int BCryptMinValue = 10;
        private const int BCryptMaxValue = 20;

        public StartupManager(
            int bCryptWorkFactor,
            [NotNull] ILogFactory logFactory)
        {
            _bCryptWorkFactor = bCryptWorkFactor;
            _log = logFactory.CreateLog(this);
        }

        public void Start()
        {
            _log.Info("Checking app settings consistency...");

            if (_bCryptWorkFactor < BCryptMinValue || _bCryptWorkFactor > BCryptMaxValue)
            {
                _log.Info($"Recommended bcrypt work factor range is [{BCryptMinValue};{BCryptMaxValue}]");

                throw new BCryptWorkFactorInconsistencyException(_bCryptWorkFactor);
            }

            _log.Info("Settings checked successfully.");
        }
    }
}
