using System.Reflection;

namespace WebAuth
{
    public static class ApplicationInformation
    {
        static ApplicationInformation()
        {
            AppVersion = Assembly.GetEntryAssembly()?.GetName()?.Version?.ToString();
        }

        public static string AppVersion { get; }
    }
}
