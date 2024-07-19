using SkgtService.Models.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SkgtService.Models;

public class ArrivalInformation
{
    public string LineName { get; set; }
    public string Start { get; set; }
    public string Direction { get; set; }
    public TransportType VehicleType { get; set; }
    public List<Arrival> Arrivals { get; set; }
    public int Minutes => Arrivals[0].Minutes;

    public ArrivalInformation(LineArrivalInfo line)
    {
        LineName = line.Name;
        //Start = line.Start;
        string[] route = line.RouteName.Split('-');
        if (route.Length > 1)
            Direction = route[1].Trim();
        else
            Direction = line.RouteName;

        Arrivals = line.Details;

        if (LineName[0] == 'E')
            VehicleType = Models.TransportType.Electrobus;
        if (LineName.Length == 3 && LineName[0] == '8' && VehicleType == Models.TransportType.Bus)
            VehicleType = Models.TransportType.Additional;

        VehicleType = line.Type;
    }

    public string LastTimings => "Следващи: " + String.Join(", ", Arrivals.Take(3).Select(t => t.Minutes));


    public string LastCalculated => Arrivals.Skip(1).Count() > 0 ? "Следващи: " + String.Join(", ", Arrivals.Skip(1).Take(3).Select(t =>
    {
        return t.Minutes + " мин";
    })) : "Последен";

    public string TransportType
    {
        get
        {
            if (LineName[0] == 'E')
                return "Електробус";
            if (LineName.Length == 3 && LineName[0] == '8' && VehicleType == Models.TransportType.Bus)
                return "Допълнителна";

            return VehicleType switch
            {
                Models.TransportType.Bus => "Автобус",
                Models.TransportType.Tram => "Трамвай",
                Models.TransportType.Trolley => "Тролей",
                _ => string.Empty
            };
        }
    }
}
