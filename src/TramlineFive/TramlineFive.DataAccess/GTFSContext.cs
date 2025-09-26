using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;
using TramlineFive.DataAccess.Entities.GTFS;

namespace TramlineFive.DataAccess;

public class GTFSContext
{
    public static string DatabasePath { get; set; }

    public static List<StopWithType> GetActiveStopsWithTypes()
    {
        SQLiteConnection db = new SQLiteConnection(DatabasePath);

        string sql = @"
        SELECT s.StopId, s.StopCode, s.StopName, s.StopLat, s.StopLon,
               GROUP_CONCAT(DISTINCT r.RouteType) as StopModes
        FROM Stop s
        INNER JOIN StopTime st ON s.StopId = st.StopId
        INNER JOIN Trip t ON st.TripId = t.TripId
        INNER JOIN Route r ON t.RouteId = r.RouteId
        GROUP BY s.StopCode
    ";

        return db.Query<StopWithType>(sql);
    }
    public static Dictionary<(string TripId, string StopId), StopTime> LoadStopTimesNexth(DateTime now, int hours)
    {
        SQLiteConnection db = new SQLiteConnection(DatabasePath);

        int nowSeconds = now.Hour * 3600 + now.Minute * 60 + now.Second;
        int endSeconds = nowSeconds + hours * 3600;

        string sql = @"
        SELECT *
        FROM StopTime
        WHERE
          (CAST(SUBSTR(DepartureTime,1,2) AS INTEGER) * 3600 +
           CAST(SUBSTR(DepartureTime,4,2) AS INTEGER) * 60 +
           CAST(SUBSTR(DepartureTime,7,2) AS INTEGER))
          BETWEEN ? AND ?
    ";

        var stopTimes = db.Query<StopTime>(sql, nowSeconds, endSeconds);

        return stopTimes.ToDictionary(st => (st.TripId, st.StopId));
    }

    public static List<Stop> GetStopsByCode(string stopCode)
    {
        SQLiteConnection db = new SQLiteConnection(DatabasePath);
        return db.Table<Stop>().Where(s => s.StopCode == stopCode).ToList();
    }

    public static Dictionary<string, int> BuildTripToVehicleTypeMap()
    {
        SQLiteConnection db = new SQLiteConnection(DatabasePath);

        string sql = @"
        SELECT t.TripId, r.RouteType
        FROM Trip t
        INNER JOIN Route r ON t.RouteId = r.RouteId
    ";

        var map = new Dictionary<string, int>();

        foreach (var row in db.Query<(string TripId, int RouteType)>(sql))
        {
            map[row.TripId] = row.RouteType;
        }

        return map;
    }


    public static List<(Route route, List<(Trip trip, StopTime stopTime)>)> 
        GetNextDeparturesPerStopQuery(string stopCode, DateTime now, int countPerRoute=3)
    {
        SQLiteConnection db = new SQLiteConnection(DatabasePath);

        var stopIds = db.Table<Stop>().Where(s => s.StopCode == stopCode).Select(s => s.StopId).ToList();
        if (stopIds.Count == 0)
            return new List<(Route, List<(Trip, StopTime)>)>();

        string nowStr = now.ToString("HH:mm:ss");

        // Single SQL query fetching everything
        var sql = @$"
SELECT r.RouteId, r.RouteShortName, r.RouteType, t.TripId, t.TripHeadsign,
       st.StopId, st.StopSequence, st.DepartureTime
FROM StopTime st
JOIN Trip t ON t.TripId = st.TripId
JOIN Route r ON r.RouteId = t.RouteId
WHERE st.StopId IN ({string.Join(',', stopIds.Select(s => $"'{s}'"))})
  AND st.DepartureTime >= ?
ORDER BY r.RouteId, st.DepartureTime";

        var departures = db.Query<StopDepartureFull>(sql, nowStr);

        // Group by route
        var grouped = departures
            .GroupBy(d => d.RouteId)
            .Select(g =>
            {
                var routeObj = new Route
                {
                    RouteId = g.Key,
                    RouteShortName = g.First().RouteShortName,
                    RouteType = g.First().RouteType
                };

                var list = g.Take(countPerRoute)
                 .Select(d =>
                 {
                     var trip = new Trip { TripId = d.TripId, TripHeadsign = d.TripHeadsign, RouteId = d.RouteId };
                     var stopTime = new StopTime { TripId = d.TripId, StopId = d.StopId, StopSequence = d.StopSequence, DepartureTime = d.DepartureTime };

                     return (trip, stopTime);
                 }).ToList();

                return (routeObj, list);
            }).ToList();

        return grouped;
    }
    public static StopTime? GetStopTime(string tripId, string stopId)
    {
        SQLiteConnection db = new SQLiteConnection(DatabasePath);
        return db.Table<StopTime>().FirstOrDefault(st => st.TripId == tripId && st.StopId == stopId);
    }

    public static void Update<T>(T entity) where T : new()
    {
        SQLiteConnection db = new SQLiteConnection(DatabasePath);
        db.Update(entity);
    }

    private static TimeSpan ParseGtfsTime(string time)
    {
        var parts = time.Split(':');
        if (parts.Length != 3) throw new FormatException($"Invalid GTFS time: {time}");

        int hours = int.Parse(parts[0]);
        int minutes = int.Parse(parts[1]);
        int seconds = int.Parse(parts[2]);

        return new TimeSpan(hours, minutes, seconds);
    }
}
