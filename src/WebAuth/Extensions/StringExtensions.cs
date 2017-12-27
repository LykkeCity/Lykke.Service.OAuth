using System.Text.RegularExpressions;
using Common;

namespace WebAuth.Extensions
{
    public static class StringExtensions
    {
        public static bool IsValidEmailAndRowKey(this string src)
        {
            return src.IsValidEmail() && !Regex.IsMatch(src, @"[\p{C}|/|\\|#|?]+");
        }
    }
}
