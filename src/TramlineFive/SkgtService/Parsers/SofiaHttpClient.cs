using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SkgtService.Parsers
{
    public class SofiaHttpClient
    {
        private HttpClient httpClient;
        private HttpClientHandler clientHandler;
        private CookieContainer cookies;

        private const string BASE_URL = "https://sofiatraffic.bg";

        public SofiaHttpClient()
        {
            cookies = new CookieContainer();
            clientHandler = new HttpClientHandler() { CookieContainer = cookies };
            httpClient = new HttpClient(clientHandler);
        }

        public async Task<HttpResponseMessage> GetAsync(string url, Dictionary<string, string> headers = null)
        {
            if (cookies.Count == 0)
            {
                await httpClient.GetAsync(BASE_URL);
            }

            Cookie token = cookies.GetAllCookies()["XSRF-TOKEN"];
            if (token == null)
            {
                return null;
            }

            HttpRequestMessage request = new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(url)
            };

            if (headers != null)
            {
                foreach (KeyValuePair<string, string> header in headers)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
            }

            request.Headers.Add("X-Xsrf-Token", Uri.UnescapeDataString(token.Value));
            return await httpClient.SendAsync(request);
        }

        public async Task<HttpResponseMessage> PostAsync(string url, HttpContent content)
        {
            if (cookies.Count == 0)
            {
                await httpClient.GetAsync(BASE_URL);
            }

            Cookie token = cookies.GetAllCookies()["XSRF-TOKEN"];
            if (token == null)
            {
                return null;
            }

            HttpRequestMessage request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                Content = content,
                RequestUri = new Uri(url)
            };


            request.Headers.Add("X-Xsrf-Token", Uri.UnescapeDataString(token.Value));
            return await httpClient.SendAsync(request);
        }

    }
}
