import 'package:sqflite/sqflite.dart';
import 'package:path/path.dart';
import 'package:trammy/models/favourite.dart';

class UserDbService {
  static Database? _db;

  static Future<Database> get database async {
    if (_db != null) return _db!;
    _db = await _initDb();
    return _db!;
  }

  static Future<Database> _initDb() async {
    final dbPath = await getDatabasesPath();
    final path = join(dbPath, 'trammy.db');

    return await openDatabase(
      path,
      version: 2, // Incremented version for the new schema
      onCreate: (db, version) async {
        await db.execute('''
          CREATE TABLE favorites (
            stop_code TEXT PRIMARY KEY,
            stop_name TEXT,
            route_type INTEGER
          )
        ''');
      },
      onUpgrade: (db, oldVersion, newVersion) async {
        if (oldVersion < 2) {
          // Migration logic if you already had an old version
          await db.execute('DROP TABLE IF EXISTS favorites');
          await db.execute('''
            CREATE TABLE favorites (
              stop_code TEXT PRIMARY KEY,
              stop_name TEXT,
              route_type INTEGER
            )
          ''');
        }
      },
    );
  }

  static Future<void> toggleFavorite(FavoriteStop fav) async {
    final db = await database;
    final exists = await isFavorite(fav.stopCode);
    
    if (exists) {
      await db.delete('favorites', where: 'stop_code = ?', whereArgs: [fav.stopCode]);
    } else {
      await db.insert('favorites', fav.toMap(), conflictAlgorithm: ConflictAlgorithm.replace);
    }
  }

  static Future<bool> isFavorite(String stopCode) async {
    final db = await database;
    final maps = await db.query('favorites', where: 'stop_code = ?', whereArgs: [stopCode]);
    return maps.isNotEmpty;
  }

  static Future<List<FavoriteStop>> getFavorites() async {
    final db = await database;
    final List<Map<String, dynamic>> maps = await db.query('favorites');
    return maps.map((m) => FavoriteStop.fromMap(m)).toList();
  }
}