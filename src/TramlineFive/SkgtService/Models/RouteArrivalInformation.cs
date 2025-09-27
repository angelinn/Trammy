using SkgtService.Models.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SkgtService.Models;

public class TripArrival
{
    public string Direction { get; set; }
    public bool Realtime { get; set; }
    public string TripId { get; set; }
    public DateTime Arrival { get; set; }
    public int MinutesTillArrival => (int)(Arrival - DateTime.Now).TotalMinutes;

}

public class RouteArrivalInformation
{
    public string LineName { get; set; }
    public string Direction => Arrivals.Count > 0 ? Arrivals[0].Direction : String.Empty;
    public TransportType VehicleType { get; set; }
    public List<TripArrival> Arrivals { get; set; } = new();
    public List<TripArrival> ArrivalsSkipFirst => Arrivals.Skip(1).ToList();
    public string RouteId { get; set; }
    public int MinutesTillArrival => Arrivals.Count > 0 ? Arrivals[0].MinutesTillArrival : 1337;
    public bool Realtime => Arrivals.Any(a => a.Realtime);
}
