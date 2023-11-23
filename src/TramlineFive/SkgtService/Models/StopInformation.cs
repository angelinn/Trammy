using SkgtService.Models.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkgtService.Models;

public class StopInformation
{
    public double Lat { get; set; }
    public double Lon { get; set; }
    public string Code { get; set; }
    public string PublicName { get; set; }
    public List<LineInformation> Lines { get; set; } = new();

    public StopInformation(StopLocation stop)
    {
        Lat = stop.Lat;
        Lon = stop.Lon;
        Code = stop.Code;
        PublicName = stop.PublicName;
    }
}
