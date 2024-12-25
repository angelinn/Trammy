using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace SkgtService.Models.Json;

public class LineArrivalInfo
{
    public int Id { get; set; }
    public string Name { get; set; }
    public TransportType Type { get; set; }
    [JsonProperty("route_name")]
    public string RouteName { get; set; }
    [JsonProperty("st_name")]
    public string StName { get; set; }
    public List<Arrival> Details { get; set; }
    [JsonProperty("last_stop")]
    public string LastStop { get; set; }
    public string LastStopName { get; set; }
}
