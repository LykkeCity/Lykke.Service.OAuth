using System;
using System.Reflection;
using System.Text;

namespace Core
{
    public static class ExceptionExtensions
    {
        public static string CollectContext(this Exception ex)
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
