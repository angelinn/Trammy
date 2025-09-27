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

    public static async Task<List<StopWithType>> GetActiveStopsWithTypesAsync()
    {
        SQLiteAsyncConnection db = new SQLiteAsyncConnection(DatabasePath);

        string sql = @"
        SELECT s.StopId, s.StopCode, s.StopName, s.StopLat, s.StopLon,
               GROUP_CONCAT(r.RouteType) as StopModes
        FROM Stop s
        INNER JOIN StopTime st ON s.StopId = st.StopId
        INNER JOIN Trip t ON st.TripId = t.TripId
        INNER JOIN Route r ON t.RouteId = r.RouteId
        GROUP BY s.StopCode
    ";

        return await db.QueryAsync<StopWithType>(sql);
    }
    public static async Task<Dictionary<(string TripId, string StopId), StopTime>> LoadStopTimesNexthAsync(DateTime now, int hours)
    {
        SQLiteAsyncConnection db = new SQLiteAsyncConnection(DatabasePath);

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

        var stopTimes = await db.QueryAsync<StopTime>(sql, nowSeconds, endSeconds);

        return stopTimes.ToDictionary(st => (st.TripId, st.StopId));
    }

    public static async Task<List<Stop>> GetStopsByCodeAsync(string stopCode)
    {
        SQLiteAsyncConnection db = new SQLiteAsyncConnection(DatabasePath);
        return await db.Table<Stop>().Where(s => s.StopCode == stopCode).ToListAsync();
    }

    public static async Task<List<StopTimeMap>> GetAllTripsAndStopsByStopCodeAsync(string stopCode)
    {
        SQLiteAsyncConnection db = new SQLiteAsyncConnection(DatabasePath);

        string sql = @"
SELECT 
    st.TripId,
    st.StopId,
    s.StopCode,
    s.StopName,
    r.RouteShortName,
    r.RouteType,
r.RouteId,
    t.TripHeadsign
FROM StopTime st
JOIN Stop s ON st.StopId = s.StopId
JOIN Trip t ON st.TripId = t.TripId
JOIN Route r ON t.RouteId = r.RouteId
WHERE s.StopCode = ?
ORDER BY st.TripId, st.StopId

";
        return await db.QueryAsync<StopTimeMap>(sql, stopCode);
    }

    public static async Task<List<(Route route, List<(Trip trip, StopTime stopTime)>)>>
        GetNextDeparturesPerStopQueryAsync(string stopCode, DateTime now, int countPerRoute=3)
    {
        SQLiteAsyncConnection db = new SQLiteAsyncConnection(DatabasePath);

        string nowStr = now.ToString("HH:mm:ss");

        // Single SQL query fetching everything
        var sql = @$"
SELECT r.RouteId, r.RouteShortName, r.RouteType, t.TripId, t.TripHeadsign,
       st.StopId, st.StopSequence, st.DepartureTime
FROM StopTime st
JOIN Trip t ON t.TripId = st.TripId
JOIN Route r ON r.RouteId = t.RouteId
JOIN Stop s on s.StopId = st.StopId
WHERE s.StopCode = ?
AND t.ServiceId IN (
    SELECT ServiceId
    FROM CalendarDate
    WHERE Date = ? AND ExceptionType = 1
)
  AND st.DepartureTime >= ?
ORDER BY r.RouteId, st.DepartureTime";

        var departures = await db.QueryAsync<StopDepartureFull>(sql, stopCode, nowStr, nowStr);

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

    public static async Task<List<(Trip trip, StopTime stopTime)>>
    GetNextDeparturesForRouteAtStopAsync(string routeId, string stopCode, DateTime now, int count = 3)
    {
        SQLiteAsyncConnection db = new SQLiteAsyncConnection(DatabasePath);

        string nowStr = now.ToString("HH:mm:ss");
        string dateStr = now.ToString("yyyyMMdd"); // CalendarDate.Date format

        var sql = @"
SELECT r.RouteId, r.RouteShortName, r.RouteType,
       t.TripId, t.TripHeadsign,
       st.StopId, st.StopSequence, st.DepartureTime
FROM StopTime st
JOIN Trip t ON t.TripId = st.TripId
JOIN Route r ON r.RouteId = t.RouteId
JOIN Stop s ON s.StopId = st.StopId
WHERE s.StopCode = ?
  AND r.RouteId = ?
  AND t.ServiceId IN (
      SELECT ServiceId
      FROM CalendarDate
      WHERE Date = ? AND ExceptionType = 1
  )
  AND st.DepartureTime >= ?
ORDER BY st.DepartureTime
LIMIT ?;";

        var departures = await db.QueryAsync<StopDepartureFull>(
            sql,
            stopCode,
            routeId,
            dateStr,
            nowStr,
            count
        );

        var result = departures.Select(d =>
        {
            var trip = new Trip
            {
                TripId = d.TripId,
                TripHeadsign = d.TripHeadsign,
                RouteId = d.RouteId
            };

            var stopTime = new StopTime
            {
                TripId = d.TripId,
                StopId = d.StopId,
                StopSequence = d.StopSequence,
                DepartureTime = d.DepartureTime
            };

            return (trip, stopTime);
        }).ToList();

        return result;
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
