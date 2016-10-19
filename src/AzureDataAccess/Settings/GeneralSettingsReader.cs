using Newtonsoft.Json;

namespace AzureDataAccess.Settings
{
    public class GeneralSettingsReader
    {
        public static T ReadSettingsFromData<T>(string jsonData)
        {
            return JsonConvert.DeserializeObject<T>(jsonData);
        }
    }
}