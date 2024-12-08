using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace SkgtService.Models.Json
{
    public class StopLocation
    {
        //[JsonProperty("latitude")]
        public double Lat => Position[0];
        //[JsonProperty("longitude")]
        public double Lon => Position[1];
        [JsonProperty("name")]
        public string PublicName { get; set; }
        [JsonProperty("code")]
        public string Code { get; set; }
        [JsonProperty("position")]
        public float[] Position { get; set; }
        [JsonProperty("type")]
        public TransportType Type { get; set; }
    }
}
