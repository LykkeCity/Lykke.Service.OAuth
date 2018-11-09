using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using Lykke.Common.ApiLibrary.Contract;
using Microsoft.Extensions.Logging;

namespace Lykke.Service.OAuth
{
    public sealed class ExceptionsHandlingConfiguration
    {
        public class HandlerConfig
        {
            public HttpStatusCode ResponseStatusCode { get; }
            public ILykkeApiErrorCode ResponseErrorCode { get; }
            public LogLevel LogLevel { get; }

            public HandlerConfig(
                HttpStatusCode responseStatusCode,
                ILykkeApiErrorCode responseErrorCode,
                LogLevel? logLevel)
            {
                ResponseErrorCode = responseErrorCode;
                ResponseStatusCode = responseStatusCode;
                LogLevel = logLevel ?? LogLevel.None;
            }
        }

        private readonly IDictionary<Type, HandlerConfig> _configurationItems;

        private ExceptionsHandlingConfiguration Add(Type exceptionType, HandlerConfig item)
        {
            _configurationItems.Add(exceptionType, item);

            return this;
        }

        public ExceptionsHandlingConfiguration()
        {
            _configurationItems = new ConcurrentDictionary<Type, HandlerConfig>();
        }

        public HandlerConfig FindConfig(Type exceptionType)
        {
            return _configurationItems.TryGetValue(exceptionType, out var result) ? result : null;
        }

        public ExceptionsHandlingConfiguration AddWarning(Type exceptionType, HttpStatusCode httpStatusCode,
            ILykkeApiErrorCode apiErrorCode)
        {
            return Add(exceptionType, new HandlerConfig(httpStatusCode, apiErrorCode, LogLevel.Warning));
        }

        public ExceptionsHandlingConfiguration AddError(Type exceptionType, HttpStatusCode httpStatusCode,
            ILykkeApiErrorCode apiErrorCode)
        {
            return Add(exceptionType, new HandlerConfig(httpStatusCode, apiErrorCode, LogLevel.Error));
        }

        public ExceptionsHandlingConfiguration AddInfo(Type exceptionType, HttpStatusCode httpStatusCode,
            ILykkeApiErrorCode apiErrorCode)
        {
            return Add(exceptionType, new HandlerConfig(httpStatusCode, apiErrorCode, LogLevel.Information));
        }

        public ExceptionsHandlingConfiguration AddCritical(Type exceptionType, HttpStatusCode httpStatusCode,
            ILykkeApiErrorCode apiErrorCode)
        {
            return Add(exceptionType, new HandlerConfig(httpStatusCode, apiErrorCode, LogLevel.Critical));
        }

        public ExceptionsHandlingConfiguration AddNoLog(Type exceptionType, HttpStatusCode httpStatusCode,
            ILykkeApiErrorCode apiErrorCode)
        {
            return Add(exceptionType, new HandlerConfig(httpStatusCode, apiErrorCode, null));
        }
    }
}
