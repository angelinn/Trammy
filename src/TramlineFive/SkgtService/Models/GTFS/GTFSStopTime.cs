using CsvHelper.Configuration.Attributes;

namespace SkgtService.Models.GTFS;

public class GTFSStopTime
{
    [Name("trip_id")]
    public string TripId { get; set; }

    [Name("arrival_time")]
    public string ArrivalTime { get; set; }  // HH:MM:SS

    [Name("departure_time")]
    public string DepartureTime { get; set; }  // HH:MM:SS

    [Name("stop_id")]
    public string StopId { get; set; }

    [Name("stop_sequence")]
    public int StopSequence { get; set; }
}
