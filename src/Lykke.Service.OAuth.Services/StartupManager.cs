using System.Collections.Generic;
using System.Linq;
using Common.Log;
using Core.Exceptions;
using Core.PasswordValidation;
using Core.Services;
using Lykke.Common.Log;

namespace Lykke.Service.OAuth.Services
{
    public class StartupManager : IStartupManager
    {
        private readonly int _bCryptWorkFactor;
        private readonly ILog _log;
        private const int BCryptMinValue = 10;
        private const int BCryptMaxValue = 20;
        private readonly IEnumerable<IPasswordValidator> _passwordValidators;

        public StartupManager(
            int bCryptWorkFactor,
            ILogFactory logFactory, 
            IEnumerable<IPasswordValidator> passwordValidators)
        {
            _bCryptWorkFactor = bCryptWorkFactor;
            _passwordValidators = passwordValidators;
            _log = logFactory?.CreateLog(this);
        }

        public void Start()
        {
            _log.Info("Checking app settings consistency...");

            if (_bCryptWorkFactor < BCryptMinValue || _bCryptWorkFactor > BCryptMaxValue)
            {
                _log.Info($"Recommended bcrypt work factor range is [{BCryptMinValue};{BCryptMaxValue}]");

                throw new BCryptWorkFactorInconsistencyException(_bCryptWorkFactor);
            }

            _log.Info("Checking pwd validators configuration...");

            if (_passwordValidators == null || !_passwordValidators.Any())
            {
                _log.Error(message: "There is no password validators configured");

                throw new NoPasswordValidatorsConfiguredException();
            }

            _log.Info("Settings checked successfully.");
        }
    }
}
