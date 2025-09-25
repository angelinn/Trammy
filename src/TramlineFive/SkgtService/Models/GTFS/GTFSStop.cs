using CsvHelper.Configuration.Attributes;

namespace SkgtService.Models.GTFS;

public class GTFSStop
{
    [Name("stop_id")]
    public string StopId { get; set; }

    [Name("stop_code")]
    public string StopCode { get; set; }

    [Name("stop_name")]
    public string StopName { get; set; }

    [Name("stop_desc")]
    public string StopDesc { get; set; }

    [Name("stop_lat")]
    public double StopLat { get; set; }

    [Name("stop_lon")]
    public double StopLon { get; set; }

    [Name("zone_id")]
    public string ZoneId { get; set; }

    [Name("stop_url")]
    public string StopUrl { get; set; }

    [Name("location_type")]
    public int? LocationType { get; set; }  // 0 = stop/platform, 1 = station, etc.

    [Name("parent_station")]
    public string ParentStation { get; set; }

    [Name("stop_timezone")]
    public string StopTimezone { get; set; }

    [Name("wheelchair_boarding")]
    public int? WheelchairBoarding { get; set; } // 0 = no info, 1 = accessible, 2 = not accessible
}