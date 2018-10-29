using System;
using Common.Log;
using Core.Exceptions;
using Core.Extensions;
using Core.Services;
using JetBrains.Annotations;
using Lykke.Common.Log;

namespace Lykke.Service.OAuth.Services
{
    /// <inheritdoc />
    public class BCryptService : IBCryptService
    {
        private readonly int _bCryptWorkFactorSettings;
        private readonly ILog _log;

        public BCryptService(
            int bCryptWorkFactorSettings,
            [NotNull] ILogFactory logFactory)
        {
            _bCryptWorkFactorSettings = bCryptWorkFactorSettings;
            _log = logFactory.CreateLog(this);
        }

        /// <inheritdoc />
        public void Verify(string source, string hash)
        {
            if (string.IsNullOrWhiteSpace(source))
                throw new ArgumentNullException(nameof(source));

            if (string.IsNullOrWhiteSpace(hash))
                throw new ArgumentNullException(nameof(hash));

            int workFactor = hash.ExtractWorkFactor();

            if (workFactor < _bCryptWorkFactorSettings)
                throw new BCryptWorkFactorOutOfRangeException(workFactor);

            bool verified;

            try
            {
                verified = BCrypt.Net.BCrypt.Verify(source, hash);
            }
            catch (Exception e)
            {
                _log.Error(e, "BCrypt library internal exception", $"source = {source}, hash = {hash}");

                throw new BCryptInternalException(e);
            }

            if (!verified)
                throw new EmailHashInvalidException(email: source);
        }
    }
}
