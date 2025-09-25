using CsvHelper.Configuration.Attributes;

namespace SkgtService.Models.GTFS;

public class GTFSTrip
{
    [Name("route_id")]
    public string RouteId { get; set; }

    [Name("service_id")]
    public string ServiceId { get; set; }

    [Name("trip_id")]
    public string TripId { get; set; }

    [Name("trip_headsign")]
    public string TripHeadsign { get; set; }

    [Name("trip_short_name")]
    public string TripShortName { get; set; }

    [Name("direction_id")]
    public int? DirectionId { get; set; }    // 0 = one direction, 1 = opposite

    [Name("block_id")]
    public string BlockId { get; set; }

    [Name("shape_id")]
    public string ShapeId { get; set; }

    [Name("wheelchair_accessible")]
    public int? WheelchairAccessible { get; set; }  // 0 = no info, 1 = accessible, 2 = not accessible

    [Name("bikes_allowed")]
    public int? BikesAllowed { get; set; }          // 0 = no info, 1 = allowed, 2 = not allowed
                                                    //}
}
