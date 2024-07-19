using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace SkgtService.Models.Json
{
    public class StopLocation
    {
        [JsonProperty("latitude")]
        public double Lat { get; set; }
        [JsonProperty("longitude")]
        public double Lon { get; set; }
        [JsonProperty("name")]
        public string PublicName { get; set; }
        [JsonProperty("code")]
        public string Code { get; set; }
    }
}
