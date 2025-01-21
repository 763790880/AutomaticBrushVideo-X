using System.Collections.Generic;
using System.Net.Http;

namespace X学堂
{
    public class Helper
    {
        private readonly IHttpClientFactory _clientFactory;
        private  HttpClient _httpClient;

        public Helper()
        {
        }
        public HttpClient CreateHttpClient()
        {
            _httpClient = new HttpClient(new HttpClientHandler
            {
                CookieContainer = new System.Net.CookieContainer()
            });
            return _httpClient;
        }
        public void SetHttpClientCookie(Dictionary<string,object> pairs)
        {
            foreach (var cookie in pairs)
            {
                _httpClient.DefaultRequestHeaders.Add(cookie.Key, (IEnumerable<string?>)cookie.Value);
                _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Cookie", $"{cookie.Key}={cookie.Value}");
            }
        }

    }
}
