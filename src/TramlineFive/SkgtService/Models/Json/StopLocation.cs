using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace SkgtService.Models.Json
{
    public class StopLocation
    {
        [JsonProperty("y")]
        public double Lat { get; set; }
        [JsonProperty("x")]
        public double Lon { get; set; }
        [JsonProperty("n")]
        public string PublicName { get; set; }
        [JsonProperty("c")]
        public string Code { get; set; }
    }
}
