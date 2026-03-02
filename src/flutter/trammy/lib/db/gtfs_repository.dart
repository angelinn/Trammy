import 'package:sqflite/sqflite.dart';
import 'package:trammy/models/gtfs/route.dart';

/// Repository for accessing GTFS data
class GTFSRepository {
  final Database db;

  GTFSRepository({required this.db});

  /// Get all stops, optionally filtered by a search string
  Future<List<Stop>> getStops({String? search}) async {
    final rows = await db.query(
      'stops',
      where: search != null ? 'stop_name LIKE ?' : null,
      whereArgs: search != null ? ['%$search%'] : null,
      orderBy: 'stop_name ASC',
    );

    return rows.map((r) => Stop.fromMap(r)).toList();
  }

  /// Get a single stop by its ID
  Future<Stop?> getStopById(String id) async {
    final rows = await db.query(
      'stops',
      where: 'stop_id = ?',
      whereArgs: [id],
      limit: 1,
    );

    if (rows.isEmpty) return null;
    return Stop.fromMap(rows.first);
  }
}