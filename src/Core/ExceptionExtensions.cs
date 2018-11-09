using System;
using System.Reflection;
using System.Text;

namespace Core
{
    /// <summary>
    /// Extension methods for <see cref="Exception"/>
    /// </summary>
    public static class ExceptionExtensions
    {
        /// <summary>
        /// Builds a formatted, line separated strings with exception's business properties
        /// </summary>
        /// <param name="ex">The exception</param>
        /// <returns></returns>
        public static string PickContext(this Exception ex)
        {
            PropertyInfo[] properties = ex.GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            StringBuilder sb = new StringBuilder();

            foreach (var propertyInfo in properties)
            {
                if (!propertyInfo.CanRead)
                    continue;

                sb.Append($"{propertyInfo.Name} = {propertyInfo.GetValue(ex)}").AppendLine();
            }

            return sb.ToString();
        }
    }
}
