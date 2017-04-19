using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Core.Http
{
    public class HttpRequestClient
    {
        public async Task<string> Request(string data, string url, string contentType = "application/json")
        {
            var oWebRequest =
                (HttpWebRequest)WebRequest.Create(url);
            oWebRequest.Method = "POST";
            oWebRequest.ContentType = contentType;

            var stream = await oWebRequest.GetRequestStreamAsync();
            var dataToSend = Encoding.UTF8.GetBytes(data);
            stream.Write(dataToSend, 0, dataToSend.Length);

            var oWebResponse = await oWebRequest.GetResponseAsync();
            var receiveStream = oWebResponse.GetResponseStream();

            try
            {
                if (receiveStream == null)
                    throw new Exception("ReceiveStream == null");

                var ms = new MemoryStream();
                receiveStream.CopyTo(ms);
                var array = ms.ToArray();

                if (array.Length > 0)
                    return Encoding.UTF8.GetString(ms.ToArray());

            }
            catch (Exception)
            {
                return string.Empty;
            }
            return string.Empty;
        }

        public async Task<string> GetRequest(string url, string contentType = "text/html")
        {
            var webRequest = (HttpWebRequest)WebRequest.Create(url);
            webRequest.Method = "GET";
            webRequest.ContentType = contentType;
            var webResponse = await webRequest.GetResponseAsync();
            using (var receiveStream = webResponse.GetResponseStream())
            {
                using (var sr = new StreamReader(receiveStream))
                {
                    return await sr.ReadToEndAsync();
                }
            }
        }
    }
}
