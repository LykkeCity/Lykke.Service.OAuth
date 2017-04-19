using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace Core.Validation
{
    public class ValidationHelper
    {
        public static void ValidateObjectRecursive<T>(T obj)
        {
            Validator.ValidateObject(obj, new ValidationContext(obj, null, null));

            var properties =
                obj.GetType().GetProperties().Where(prop => prop.CanRead && !prop.GetIndexParameters().Any()).ToList();

            foreach (var property in properties)
            {
                if ((property.PropertyType == typeof(string))
                    || (property.PropertyType == typeof(decimal))
                    || property.PropertyType.GetTypeInfo().IsValueType
                    || property.PropertyType.GetTypeInfo().IsPrimitive
                    || property.PropertyType.GetTypeInfo().IsEnum)
                {
                    continue;
                }

                var value = property.GetValue(obj);

                if (value == null)
                {
                    continue;
                }

                ValidateObjectRecursive(value);
            }
        }
    }
}
