using SkgtService.Models.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkgtService.Models;

public class StopInformation
{
    public double Lat { get; set; }
    public double Lon { get; set; }
    public string Code { get; set; }

    private string publicName;
    public string PublicName
    {
        get => publicName.ToUpper();
        set => publicName = value;
    }

    public TransportType Type { get; set; }

    public StopInformation()
    {

    }

    public StopInformation(StopLocation stop)
    {
        Lat = stop.Lat;
        Lon = stop.Lon;
        Code = stop.Code;
        PublicName = stop.PublicName;
        Type = stop.Type;
    }
}

public class StopResponse
{
    public string Code { get; set; }
    public string PublicName { get; set; }
    public ObservableCollection<RouteArrivalInformation> Arrivals { get; set; } = new();
    public bool IsFavourite { get; set; }

    public StopResponse(string stopCode, string name)
    {
        Code = stopCode;
        PublicName = name;
    }
}
