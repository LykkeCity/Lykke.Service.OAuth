using System.Text.RegularExpressions;
using Common;

namespace WebAuth.Extensions
{
    public static class StringExtensions
    {
        public static bool IsValidEmailAndRowKey(this string src)
        {
            return src.IsValidEmail() && src.IsValidRowKey();
        }
        
        public static bool IsValidRowKey(this string src)
        {
            return !string.IsNullOrEmpty(src) && !Regex.IsMatch(src, @"[\p{C}|/|\\|#|?]+");
        }
    }
}
