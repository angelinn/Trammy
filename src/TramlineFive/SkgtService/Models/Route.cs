using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkgtService.Models
{
    public class Way
    {
        public List<string> Codes { get; set; }
        public string Last => StopsLoader.StopsHash[Codes[^1]].PublicName;
        public string First => StopsLoader.StopsHash[Codes[0]].PublicName;
    }

    public class Route
    {
        public string Name { get; set; }
        public List<Way> Routes { get; set; }
    }

    public class Routes
    {
        public string Type{ get; set; }
        public List<Route> Lines { get; set; }
    }
}
