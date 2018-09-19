using System;
using System.Linq;

namespace Common
{
    public static class IdGenerator
    {
        public static string GenerateDateTimeIdNewFirst(DateTime creationDateTime)
        {
            return $"{(DateTime.MaxValue.Ticks - creationDateTime.Ticks):d19}_{Guid.NewGuid():N}";
        }
    }

    public static class StringExtensions
    {
        public static bool IsValidEmail(this string email)
        {
            if (string.IsNullOrEmpty(email))
                return false;

            email = email.Trim();

            var lines = email.Split('@');

            if (lines.Length != 2)
                return false;

            if (lines[0].Trim() == "" || lines[1].Trim() == "")
                return false;

            if (lines[0].Contains(' ') || lines[1].Contains(' '))
                return false;

            var lines2 = lines[1].Split('.');

            return lines2.Length >= 2;
        }
    }
}
