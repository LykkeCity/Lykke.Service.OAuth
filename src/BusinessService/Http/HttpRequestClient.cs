using System.Threading.Tasks;
using RestSharp;

namespace BusinessService.Http
{
    public class HttpRequestClient
    {
        public async Task<string> GetRequest(string url)
        {
            var restClient = new RestClient(url);

            var request = new RestRequest("", Method.GET);
            request.AddHeader("Accept", "application/json");

            var taskCompletion = new TaskCompletionSource<IRestResponse>();

            restClient.ExecuteAsync(request, r => taskCompletion.SetResult(r));

            var response = (RestResponse)(await taskCompletion.Task);

            return response.Content;
        }

        public async Task<string> PostRequest(string data, string url)
        {
            var restClient = new RestClient(url);

            var request = new RestRequest("", Method.POST);
            request.AddHeader("Accept", "application/json");
            
            request.AddParameter("application/json", data, ParameterType.RequestBody);

            var taskCompletion = new TaskCompletionSource<IRestResponse>();

            restClient.ExecuteAsync(request, r => taskCompletion.SetResult(r));

            var response = (RestResponse)(await taskCompletion.Task);

            return response.Content;
        }
    }
}
