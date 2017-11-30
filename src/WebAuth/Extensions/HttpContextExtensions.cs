using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace WebAuth.Extensions
{
    public static class HttpContextExtensions
    {
        public static string GetIp(this HttpContext ctx)
        {
            string ip = string.Empty;

            // http://stackoverflow.com/a/43554000/538763
            var xForwardedForVal = GetHeaderValueAs<string>(ctx, "X-Forwarded-For").SplitCsv().FirstOrDefault();

            if (!string.IsNullOrEmpty(xForwardedForVal))
            {
                ip = xForwardedForVal.Split(':')[0];
            }

            // RemoteIpAddress is always null in DNX RC1 Update1 (bug).
            if (string.IsNullOrWhiteSpace(ip) && ctx?.Connection?.RemoteIpAddress != null)
                ip = ctx.Connection.RemoteIpAddress.ToString();

            if (string.IsNullOrWhiteSpace(ip))
                ip = GetHeaderValueAs<string>(ctx, "REMOTE_ADDR");

            return ip;
        }

        public static string GetReferer(this HttpContext ctx)
        {
            return GetHeaderValueAs<string>(ctx, "Referer");
        }

        public static string GetUserAgent(this HttpContext ctx)
        {
            return GetHeaderValueAs<string>(ctx, "User-Agent");
        }

        public static string GetApplicationId(this HttpContext ctx)
        {
            return GetHeaderValueAs<string>(ctx, "application_id");
        }

        #region Tools

        private static T GetHeaderValueAs<T>(HttpContext httpContext, string headerName)
        {
            StringValues values;

            if (httpContext?.Request?.Headers?.TryGetValue(headerName, out values) ?? false)
            {
                string rawValues = values.ToString();   // writes out as Csv when there are multiple.

                if (!string.IsNullOrEmpty(rawValues))
                    return (T)Convert.ChangeType(values.ToString(), typeof(T));
            }
            return default(T);
        }

        private static List<string> SplitCsv(this string csvList, bool nullOrWhitespaceInputReturnsNull = false)
        {
            if (string.IsNullOrWhiteSpace(csvList))
                return nullOrWhitespaceInputReturnsNull ? null : new List<string>();

            return csvList
                .TrimEnd(',')
                .Split(',')
                .AsEnumerable()
                .Select(s => s.Trim())
                .ToList();
        }

        #endregion
    }
}
