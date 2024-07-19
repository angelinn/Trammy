using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    }
}
