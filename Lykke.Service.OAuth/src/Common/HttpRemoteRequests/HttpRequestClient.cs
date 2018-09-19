using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Common.HttpRemoteRequests
{
    public class HttpRequestClient
    {
        public async Task<string> Request(string data, string url, string contentType = "application/json")
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(contentType));

                var dataToSend = Encoding.UTF8.GetBytes(data);
                var byteContent = new ByteArrayContent(dataToSend);

                var oWebResponse = await client.PostAsync(url, byteContent);
                var receiveStream = await oWebResponse.Content.ReadAsStreamAsync();

                try
                {
                    if (receiveStream == null)
                        throw new Exception("ReceiveStream == null");

                    var ms = new MemoryStream();
                    receiveStream.CopyTo(ms);
                    var array = ms.ToArray();

                    if (array.Length > 0)
                        return Encoding.UTF8.GetString(array);
                }
                catch (Exception)
                {
                    return string.Empty;
                }
                return string.Empty;
            }
        }

        public async Task<string> GetRequest(string url)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));

                var oWebResponse = await client.GetAsync(url);

                if ((int) oWebResponse.StatusCode == 201)
                    return null;

                return await oWebResponse.Content.ReadAsStringAsync();
            }
        }
    }
}