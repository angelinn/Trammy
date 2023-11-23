using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkgtService.Models;

public class LineInformation
{
    public string Name { get; set; }
    public TransportType VehicleType { get; set; }
    public string Test => "TEST";

    public LineInformation(string name, string vehicleType)
    {
        Name = name;
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
    }
}
