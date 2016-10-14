using System;

namespace Common
{
    public static class IdGenerator
    {
        public static string GenerateDateTimeIdNewFirst(DateTime creationDateTime)
        {
            return $"{(DateTime.MaxValue.Ticks - creationDateTime.Ticks).ToString("d19")}_{Guid.NewGuid().ToString("N")}";
        }
    }
}
