using Newtonsoft.Json;
using SkgtService.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SkgtService.Parsers
{
    public class ArrivalsService
    {
        private const string ARRIVALS_API_URL = @"https://api-arrivals.sofiatraffic.bg/api/v1/arrivals";
        private const string OPERA_USER_AGENT = @"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/61.0.3163.100 Safari/537.36 OPR/48.0.2685.35";

        private readonly HttpClient client = new HttpClient();

        public ArrivalsService()
        {
            client.DefaultRequestHeaders.Add("User-Agent", OPERA_USER_AGENT);
        }

        public async Task<StopInfo> GetByStopCodeAsync(string stopCode)
        {
            HttpResponseMessage message = await client.GetAsync($"{ARRIVALS_API_URL}/{stopCode}/");
            string json = await message.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<StopInfo>(json);
        }
    }
}
