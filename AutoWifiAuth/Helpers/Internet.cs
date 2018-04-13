using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;
using System.Net;

namespace AutoWifiAuth.Helpers
{
    public class Internet
    {
        private HttpClient http;

        private static Internet instance;

        public static Internet GetInstance()
        {
            if(instance == null)
            {
                instance = new Internet();
            }
            return instance;
        }

        private Internet()
        {
            Cookies = new CookieContainer();
            var handler = new HttpClientHandler() { UseCookies = true, CookieContainer = Cookies };
            http = new HttpClient(handler)
            {
                MaxResponseContentBufferSize = 10240000
            };
            http.DefaultRequestHeaders.ExpectContinue = false;
        }

        public CookieContainer Cookies { get; private set; }

        public async Task<HttpContent> PostAsync(string link, IEnumerable<KeyValuePair<string, string>> form)
        {
            var content = new FormUrlEncodedContent(form);
            var response = await http.PostAsync(link, content);
            return response.Content;
        }

        public async Task<HttpContent> GetAsync(Uri link)
        {
            var response = await http.GetAsync(link);
            return response.Content;
        }

        public async Task<HttpContent> GetAsync(string link)
        {
            var response = await http.GetAsync(link);
            return response.Content;
        }
        public async Task<Stream> GetAsyncStream(Uri link)
        {
            var response = await http.GetAsync(link);
            return await response.Content.ReadAsStreamAsync();
        }
    }
}


