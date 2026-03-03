import 'package:sqflite/sqflite.dart';
import 'package:trammy/db/gtfs_db_builder.dart';
import 'package:trammy/models/gtfs/route.dart';
import 'package:trammy/models/gtfs/stop.dart';
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
