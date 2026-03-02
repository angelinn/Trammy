import 'package:sqflite/sqflite.dart';
import 'package:trammy/models/gtfs/route.dart';
import 'package:trammy/models/gtfs/stop.dart';

/// Repository for accessing GTFS data
class GTFSRepository {
  final Database db;

  GTFSRepository({required this.db});

  /// Get all stops, optionally filtered by a search string
  Future<List<GTFSStop>> getStops() async {
    final rows = await db.query(
      'stops',
      where: 'stop_lat is NOT NULL AND stop_lon is NOT NULL',
      orderBy: 'stop_name ASC',
    );

    return rows.map((r) => GTFSStop.fromMap(r)).toList();
  }

  /// Get a single stop by its ID
  Future<GTFSStop?> getStopById(String id) async {
    final rows = await db.query(
      'stops',
      where: 'stop_id = ?',
      whereArgs: [id],
      limit: 1,
    );

    if (rows.isEmpty) return null;
    return GTFSStop.fromMap(rows.first);
  }
}