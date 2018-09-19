using System;
using Common.Log;

namespace Common.Validation
{
    public class GeneralSettingsValidator
    {
        public static void Validate<T>(T settings, ILog log = null)
        {
            try
            {
                ValidationHelper.ValidateObjectRecursive(settings);
            }
            catch (Exception e)
            {
                log?.WriteFatalErrorAsync("GeneralSettings", "Validation", null, e);

                throw;
            }
        }
    }
}