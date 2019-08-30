using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace NateK.Lib
{

    public interface IHttpService
    {
        Task<T> Get<T>(string url);
    }

    public class HttpService : IHttpService
    {
        private HttpClient _httpClient;

        public HttpService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<T> Get<T>(string url)
        {
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsAsync<T>();
            return result;
        }
    }
}
