using SkgtService.Models.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkgtService.Models;

public class LineRoute
{
    public List<string> Codes { get; set; }

    public LineRoute(Way way)
    {
        Codes = way.Codes;
    }
}

public class LineInformation
{
    public string Name { get; set; }
    public TransportType VehicleType { get; set; }
    public List<LineRoute> Routes { get; set; }


    public LineInformation(string name, string vehicleType, List<Way> routes)
    {
        Name = name.Replace("E", "Е").Replace("TM", "-ТМ");
        VehicleType = vehicleType switch
        {
            "bus" => TransportType.Bus,
            "trolley" => TransportType.Trolley,
            "tram"=> TransportType.Tram,
            _ => TransportType.Bus
        };

        // latin or cyrilic
        if (name.StartsWith('E') || name.StartsWith('Е'))
            VehicleType = TransportType.Electrobus;
        else if (Name.Length == 3 && Name.StartsWith('8') && VehicleType == TransportType.Bus)
            VehicleType = TransportType.Additional;

        Routes = routes.Select(r => new LineRoute(r)).ToList();
    }
}
