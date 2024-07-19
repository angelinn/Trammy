using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SkgtService.Models.Json;

public class Arrival
{
    [JsonProperty("t")]
    public int Minutes { get; set; }
    public bool AC { get; set; }
    public bool Bikes { get; set; }
}
