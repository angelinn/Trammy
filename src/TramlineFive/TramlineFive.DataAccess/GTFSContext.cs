using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
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

    public static async Task<List<StopWithType>> FetchDominantStops()
    {
        SQLiteAsyncConnection db = new SQLiteAsyncConnection(DatabasePath);

        string sql = @"
SELECT 
    d.StopCode,
    AVG(s.StopLat) AS StopLat,
    AVG(s.StopLon) AS StopLon,
    s.StopName,
    s.StopId,
    d.DominantRouteType
FROM StopDominantType d
JOIN Stop s ON s.StopCode = d.StopCode
GROUP BY d.StopCode;";

        return await db.QueryAsync<StopWithType>(sql);
    }

    public static  int CreateDominantTypesTable(SQLiteConnection db)
    {
        string sql = @"
CREATE TABLE StopDominantType AS
WITH RouteCounts AS (
    SELECT 
        s.StopCode,
        r.RouteType,
        COUNT(*) AS RouteTypeCount
    FROM Stop s
    JOIN StopTime st ON s.StopId = st.StopId
    JOIN Trip t ON st.TripId = t.TripId
    JOIN Route r ON t.RouteId = r.RouteId
    GROUP BY s.StopCode, r.RouteType
),
Dominant AS (
    SELECT 
        StopCode,
        RouteType AS DominantRouteType
    FROM (
        SELECT 
            StopCode,
            RouteType,
            RouteTypeCount,
            ROW_NUMBER() OVER (
                PARTITION BY StopCode 
                ORDER BY RouteTypeCount DESC
            ) AS rn
        FROM RouteCounts
    ) ranked
    WHERE rn = 1
)
SELECT 
    d.StopCode,
    d.DominantRouteType
FROM Dominant d
GROUP BY d.StopCode;";
        return 
            db.Execute(sql);

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

        var departures = await db.QueryAsync<StopDepartureFull>(sql, stopCode, now.ToString("yyyyMMdd"), now.ToString("HH:mm:ss"));

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
      WHERE Date = strftime('%Y%m%d','now','localtime')  AND ExceptionType = 1
  )
  AND st.DepartureTime >= ?
ORDER BY st.DepartureTime;";

        var departures = await db.QueryAsync<StopDepartureFull>(
            sql,
            stopCode,
            routeId,
            nowStr
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

    public static async Task CreateDatabaseFromStaticDataAsync(string extractPath, Action<CreateDatabaseCallbackParams> progressCallback)
    {
        File.Delete(DatabasePath);

        SQLiteConnection db = new SQLiteConnection(DatabasePath);

        db.CreateTable<Stop>();
        db.CreateTable<Route>();
        db.CreateTable<Trip>();
        db.CreateTable<StopTime>();
        db.CreateTable<CalendarDate>();
        db.Execute("CREATE UNIQUE INDEX IF NOT EXISTS IX_Trip_Stop ON StopTime(TripId, StopId)");

        // Insert CSVs
        await InsertCsvAsync<Stop>(db, Path.Combine(extractPath, "stops.txt"), "stops", progressCallback);
        await InsertCsvAsync<Route>(db, Path.Combine(extractPath, "routes.txt"), "routes", progressCallback);
        await InsertCsvAsync<Trip>(db, Path.Combine(extractPath, "trips.txt"), "trips", progressCallback);
        await InsertCsvAsync<CalendarDate>(db, Path.Combine(extractPath, "calendar_dates.txt"), "calendar_dates", progressCallback);
        await InsertCsvAsync<StopTime>(db, Path.Combine(extractPath, "stop_times.txt"), "stop_times", progressCallback);

        progressCallback(new CreateDatabaseCallbackParams(0, "Оптимизиране на зареждане спирки..."));

        CreateDominantTypesTable(db);
    }


    private static async Task InsertCsvAsync<T>(SQLiteConnection db, string filePath, string tableName, Action<CreateDatabaseCallbackParams> progressCallback)
    {
        using StreamReader reader = new StreamReader(filePath);
        using CsvReader csv = new CsvReader(reader, new CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HeaderValidated = null,       // Ignore missing header validation
            PrepareHeaderForMatch = (args) => args.Header.Replace("_", "").ToLower() // Normalize
        });

        progressCallback(new CreateDatabaseCallbackParams(0, $"Четене на {tableName}..."));

        List<T> records = csv.GetRecords<T>().ToList();
        int total = records.Count;
        int count = 0;

        await Task.Run(() =>
        {
            db.RunInTransaction(() =>
            {
                foreach (T record in records)
                {
                    db.InsertOrReplace(record);
                    count++;
                    if (count % 100 == 0) // update UI every 100 records
                    {
                        progressCallback(new CreateDatabaseCallbackParams((double)count / total, $"Въвеждане на {tableName}: {count}/{total}"));
                    }
                }
            });
        });

        progressCallback(new CreateDatabaseCallbackParams(1, $"Готово {tableName}"));
    }

    public record CreateDatabaseCallbackParams(double Progress, string Status);
}
