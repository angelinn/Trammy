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
    }

    public class Route
    {
        public string Name { get; set; }
        public List<Way> Routes { get; set; }
    }

    public class MainRouteObject
    {
        public List<Route> Lines { get; set; }
    }
}
