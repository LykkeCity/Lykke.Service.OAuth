using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using Lykke.Common.ApiLibrary.Contract;
using Microsoft.Extensions.Logging;

namespace Lykke.Service.OAuth
{
    /// <summary>
    /// Configuration of exception handlers
    /// </summary>
    public sealed class ExceptionsHandlingConfiguration
    {
        /// <summary>
        /// Single exception handler configuration
        /// </summary>
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

        /// <summary>
        /// Finds the handling configuration for exception
        /// </summary>
        /// <param name="exceptionType">Exception type</param>
        /// <returns>Exception handler details</returns>
        public HandlerConfig Find(Type exceptionType)
        {
            return _configurationItems.TryGetValue(exceptionType, out var result) ? result : null;
        }

        /// <summary>
        /// Registers exception handling configuration with warning log level
        /// </summary>
        /// <param name="exceptionType">Exception type</param>
        /// <param name="httpStatusCode">HttpStatusCode to respond with</param>
        /// <param name="apiErrorCode">Business error code to respond with</param>
        /// <returns>Configuration of exception handlers</returns>
        public ExceptionsHandlingConfiguration AddWarning(Type exceptionType, HttpStatusCode httpStatusCode,
            ILykkeApiErrorCode apiErrorCode)
        {
            return Add(exceptionType, new HandlerConfig(httpStatusCode, apiErrorCode, LogLevel.Warning));
        }

        /// <summary>
        /// Registers exception handling configuration with error log level
        /// </summary>
        /// <param name="exceptionType">Exception type</param>
        /// <param name="httpStatusCode">HttpStatusCode to respond with</param>
        /// <param name="apiErrorCode">Business error code to respond with</param>
        /// <returns>Configuration of exception handlers</returns>
        public ExceptionsHandlingConfiguration AddError(Type exceptionType, HttpStatusCode httpStatusCode,
            ILykkeApiErrorCode apiErrorCode)
        {
            return Add(exceptionType, new HandlerConfig(httpStatusCode, apiErrorCode, LogLevel.Error));
        }

        /// <summary>
        /// Registers exception handling configuration with info log level
        /// </summary>
        /// <param name="exceptionType">Exception type</param>
        /// <param name="httpStatusCode">HttpStatusCode to respond with</param>
        /// <param name="apiErrorCode">Business error code to respond with</param>
        /// <returns>Configuration of exception handlers</returns>
        public ExceptionsHandlingConfiguration AddInfo(Type exceptionType, HttpStatusCode httpStatusCode,
            ILykkeApiErrorCode apiErrorCode)
        {
            return Add(exceptionType, new HandlerConfig(httpStatusCode, apiErrorCode, LogLevel.Information));
        }

        /// <summary>
        /// Registers exception handling configuration with critical log level
        /// </summary>
        /// <param name="exceptionType">Exception type</param>
        /// <param name="httpStatusCode">HttpStatusCode to respond with</param>
        /// <param name="apiErrorCode">Business error code to respond with</param>
        /// <returns>Configuration of exception handlers</returns>
        public ExceptionsHandlingConfiguration AddCritical(Type exceptionType, HttpStatusCode httpStatusCode,
            ILykkeApiErrorCode apiErrorCode)
        {
            return Add(exceptionType, new HandlerConfig(httpStatusCode, apiErrorCode, LogLevel.Critical));
        }

        /// <summary>
        /// Registers exception handling configuration
        /// </summary>
        /// <param name="exceptionType">Exception type</param>
        /// <param name="httpStatusCode">HttpStatusCode to respond with</param>
        /// <param name="apiErrorCode">Business error code to respond with</param>
        /// <returns>Configuration of exception handlers</returns>
        public ExceptionsHandlingConfiguration AddNoLog(Type exceptionType, HttpStatusCode httpStatusCode,
            ILykkeApiErrorCode apiErrorCode)
        {
            return Add(exceptionType, new HandlerConfig(httpStatusCode, apiErrorCode, null));
        }
    }
}
