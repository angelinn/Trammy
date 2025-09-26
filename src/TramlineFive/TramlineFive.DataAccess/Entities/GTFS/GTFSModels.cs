using System;
using SQLite;

namespace TramlineFive.DataAccess.Entities.GTFS;
public class Stop
{
    [PrimaryKey] public string StopId { get; set; }
    [Indexed] public string StopCode { get; set; }
    public string StopName { get; set; }
    public double StopLat { get; set; }
    public double StopLon { get; set; }
}

public class Route
{
    [PrimaryKey] public string RouteId { get; set; }
    public int RouteType { get; set; }
    public string RouteShortName { get; set; }
    public string RouteLongName { get; set; }
}

public class Trip
{
    [PrimaryKey] public string TripId { get; set; }
    [Indexed] public string RouteId { get; set; }
    public string ServiceId { get; set; }
    public string TripHeadsign { get; set; }
}

public class StopTime
{
    public string TripId { get; set; }
    [Indexed] public string StopId { get; set; }
    public int StopSequence { get; set; }
    public string ArrivalTime { get; set; }
    public string DepartureTime { get; set; }

}

public class StopDepartureFull
{
    public string RouteId { get; set; }
    public string RouteShortName { get; set; }
    public int RouteType { get; set; }
    public string TripId { get; set; }
    public string TripHeadsign { get; set; }
    public string StopId { get; set; }
    public int StopSequence { get; set; }
    public string DepartureTime { get; set; }
}

public class StopWithType
{
    public string StopId { get; set; }
    public string StopCode { get; set; }
    public string StopName { get; set; }
    public double StopLat { get; set; }
    public double StopLon { get; set; }

    // aggregated info
    public string StopModes { get; set; } // e.g. "Bus, Tram"
}