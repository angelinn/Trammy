using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace SkgtService.Models.Json
{
    public class Way
    {
        public List<string> Codes { get; set; }
    }

    public class Route
    {
        public string Name { get; set; }
        public List<Way> Routes { get; set; }
    }

    public class Routes
    {
        public string Type { get; set; }
        public List<Route> Lines { get; set; }
    }

    public class Line
    {
        public string Name { get; set; }
        public TransportType Type { get; set; }
        public string Color { get; set; }
        [JsonProperty("ext_id")]
        public string ExtId { get; set; }
        public string Icon { get; set; }
        public int IsWeekend { get; set; }
        [JsonProperty("line_id")]
        public int LineID { get; set; }


    }
}
