import 'package:sqflite/sqflite.dart';
import 'package:trammy/db/gtfs_db_builder.dart';
import 'package:trammy/models/gtfs/route.dart';
import 'package:trammy/models/gtfs/stop.dart';
import 'package:trammy/models/gtfs/stop_time.dart';
import 'package:trammy/models/gtfs/trip.dart';
import 'package:trammy/services/gtfs_service.dart';

/// Repository for accessing GTFS data
class GTFSRepository {
  late GtfsDbBuilder dbBuilder;

  bool initialized = false;

  Future<void> initialize(String path) async {
    if (initialized) return;
    initialized = true;

    dbBuilder = GtfsDbBuilder();
    await dbBuilder.initialize(path);
  }

  Future<void> updateGTFS({
    required void Function(GTFSProgress progress) onProgress,
    required String workingDirectory,
  }) async {
    await dbBuilder.updateGTFS(
      onProgress: onProgress,
      workingDirectory: workingDirectory,
    );
  }

  /// Get all stops, optionally filtered by a search string
  Future<List<GTFSStop>> getStopsRaw() async {
    final rows = await dbBuilder.db.query(
      'stops',
      where: 'stop_lat is NOT NULL AND stop_lon is NOT NULL',
      orderBy: 'stop_name ASC',
    );

    return rows.map((r) => GTFSStop.fromMap(r)).toList();
  }

  Future<List<GTFSStopRouteInfo>> getStops() async {
    final rows = await dbBuilder.db.query('stop_route_info', orderBy: 'stop_name ASC');
    return rows.map((r) => GTFSStopRouteInfo.fromMap(r)).toList();
  }

  Future<List<GTFSRoute>> getRoutes() async {
    final rows = await dbBuilder.db.query('routes', orderBy: 'route_short_name ASC');

    return rows.map((r) => GTFSRoute.fromMap(r)).toList();
  }

  Future<List<GTFSTrip>> getTrips() async {
    final rows = await dbBuilder.db.query('trips');

    return rows.map((r) => GTFSTrip.fromMap(r)).toList();
  }

  Future<List<GTFSStopTimeData>> getStopTimesAfterNow(String stopCode, int limit) async {

final today = DateTime.now();
final upperLimit = today.add(const Duration(hours: 2));
final yyyymmdd =
    "${today.year.toString().padLeft(4,'0')}"
    "${today.month.toString().padLeft(2,'0')}"
    "${today.day.toString().padLeft(2,'0')}";
final hhmmss =
        "${today.hour.toString().padLeft(2, '0')}:"
         "${today.minute.toString().padLeft(2, '0')}:"
         "${today.second.toString().padLeft(2, '0')}";
final hhmmssUpper =
        "${upperLimit.hour.toString().padLeft(2, '0')}:"
         "${upperLimit.minute.toString().padLeft(2, '0')}:"
         "${upperLimit.second.toString().padLeft(2, '0')}";
final rows = await dbBuilder.db.rawQuery(
  '''
  SELECT
      st.trip_id,
      st.stop_id,
      st.arrival_time,
      st.departure_time,
      st.stop_sequence,
      t.route_id,
      t.trip_headsign,
      r.route_short_name,
      r.route_type,
      r.route_color
  FROM stop_times st
  JOIN stops s ON st.stop_id = s.stop_id
  JOIN trips t ON st.trip_id = t.trip_id
  JOIN routes r ON t.route_id = r.route_id
  JOIN calendar_dates cd ON t.service_id = cd.service_id
  WHERE s.stop_code = ?
    AND cd.date = ?
    AND cd.exception_type = 1
    AND st.departure_time >= ?
    AND st.departure_time <= ?
  ORDER BY st.arrival_time;
  ''',
  [stopCode, yyyymmdd, hhmmss, hhmmssUpper],);

    return rows.map((r) => GTFSStopTimeData.fromMap(r)).toList();
  }

  /// Get a single stop by its ID
  Future<GTFSStop?> getStopById(String id) async {
    final rows = await dbBuilder.db.query(
      'stops',
      where: 'stop_id = ?',
      whereArgs: [id],
      limit: 1,
    );

    if (rows.isEmpty) return null;
    return GTFSStop.fromMap(rows.first);
  }
}
