using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace SkgtService.Models.Json;

public class Route1
{
    public Direction Direction0 { get; set; }
    public Direction Direction1 { get; set; }
    public string Line { get; set; }
}

public class Direction
{
    public string Name { get; set; }
    public List<Segment1> Segments { get; set; }

}

public class Segment1
{
    public string StartStop { get; set; }
    public string EndStop { get; set; }
    public string Name { get; set; }
    public string Wkt { get; set; }
}
