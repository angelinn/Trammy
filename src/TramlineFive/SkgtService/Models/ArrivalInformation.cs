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

    public ArrivalInformation(Line line)
    {
        LineName = line.Name;
        Start = line.Start;
        Direction = line.Direction;
        Arrivals = line.Arrivals;

        if (LineName[0] == 'E')
            VehicleType = Models.TransportType.Electrobus;
        if (LineName.Length == 3 && LineName[0] == '8' && VehicleType == Models.TransportType.Bus)
            VehicleType = Models.TransportType.Additional;

        char[] vehicleType = line.VehicleType.ToCharArray();
        vehicleType[0] = char.ToUpper(vehicleType[0]);

        Enum.TryParse(typeof(TransportType), vehicleType, out object type);
        VehicleType = (TransportType)type;
    }

    public string LastTimings => "Следващи: " + String.Join(", ", Arrivals.Take(3).Select(t => t.Time));


    public string LastCalculated => Arrivals.Skip(1).Count() > 0 ? "Следващи: " + String.Join(", ", Arrivals.Skip(1).Take(3).Select(t =>
    {
        TimeSpan arrival = DateTime.Parse(t.Time) - DateTime.Now;
        int minutes = arrival.Minutes;

        if (arrival.Hours > 0)
            minutes += arrival.Hours * 60;

        return minutes + " мин";
    })) : "Последен";

    public int Minutes
    {
        get
        {
            DateTime closest = DateTime.Parse(Arrivals[0].Time);

            int minutes = (int)Math.Round((closest - DateTime.Now).TotalMinutes);
            if (minutes < 0)
            {
                if (Arrivals.Count > 1)
                {
                    closest = DateTime.Parse(Arrivals[1].Time);
                    minutes = (int)Math.Round((closest - DateTime.Now).TotalMinutes);
                }
                else
                    minutes = 0;
            }

            return minutes;
        }
    }
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
