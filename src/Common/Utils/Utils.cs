using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public static class Utils
    {
        private static readonly Dictionary<byte, string> Hex0 = new Dictionary<byte, string>
        {
            {0, "0"},
            {1, "1"},
            {2, "2"},
            {3, "3"},
            {4, "4"},
            {5, "5"},
            {6, "6"},
            {7, "7"},
            {8, "8"},
            {9, "9"},
            {10, "A"},
            {11, "B"},
            {12, "C"},
            {13, "D"},
            {14, "E"},
            {15, "F"},
        };

        public const string IsoDateTimeMask = "yyyy-MM-dd HH:mm:ss";

        public const string IsoDateMask = "yyyy-MM-dd";
        public const string RussianDateMask = "dd.MM.yyyy";

        public static T ParseEnum<T>(this string value)
        {
            return (T) Enum.Parse(typeof(T), value, true);
        }

        public static IEnumerable<IEnumerable<T>> ToPieces<T>(this IEnumerable<T> src, int countInPicese)
        {
            var result = new List<T>();

            foreach (var itm in src)
            {
                result.Add(itm);
                if (result.Count >= countInPicese)
                {
                    yield return result;
                    result = new List<T>();
                }
            }

            if (result.Count > 0)
            {
                yield return result;
            }
        }

        public static byte[] ToBytes(this Stream src)
        {
            var memoryStream = src as MemoryStream;

            if (memoryStream != null)
            {
                return memoryStream.ToArray();
            }


            src.Position = 0;
            var result = new MemoryStream();

            src.CopyTo(result);
            return result.ToArray();
        }

        public static async Task<byte[]> ToBytesAsync(this Stream src)
        {
            var memoryStream = src as MemoryStream;

            if (memoryStream != null)
            {
                return memoryStream.ToArray();
            }


            var result = new MemoryStream();
            await src.CopyToAsync(result);
            return result.ToArray();
        }

        public static byte[] ToUtf8Bytes(this string src)
        {
            return Encoding.UTF8.GetBytes(src);
        }

        public static Stream ToStream(this byte[] src)
        {
            if (src == null)
            {
                return null;
            }

            return new MemoryStream(src) {Position = 0};
        }

        public static string ToHexString(this ICollection<byte> src)
        {
            var sb = new StringBuilder(src.Count * 2);

            foreach (var b in src)
                sb.Append(ByteToHex(b));

            return sb.ToString();
        }

        public static string ByteToHex(byte src)
        {
            var d2 = (byte)(src * 0.0625);
            src = (byte)(src - d2 * 16);

            return Hex0[d2] + Hex0[src];
        }
    }
}