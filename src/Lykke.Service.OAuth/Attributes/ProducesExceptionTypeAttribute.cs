using System;

namespace Lykke.Service.OAuth.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class ProducesExceptionTypeAttribute : Attribute
    {
        public Type ExceptionType { get; set; }

        public ProducesExceptionTypeAttribute(Type exceptionType)
        {
            ExceptionType = exceptionType ?? throw new ArgumentNullException(nameof(exceptionType));
        }
    }
}
