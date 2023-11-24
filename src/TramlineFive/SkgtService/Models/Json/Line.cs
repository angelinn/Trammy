using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SkgtService.Models.Json;

public class Line
{
    public string Name { get; set; }
    public string Start { get; set; }
    public string Direction { get; set; }
    [JsonProperty("vehicle_type")]
    public string VehicleType { get; set; }
    public List<Arrival> Arrivals { get; set; } 
}
