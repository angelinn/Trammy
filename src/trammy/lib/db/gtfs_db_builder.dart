import 'dart:async';
import 'dart:convert';
import 'dart:io';

import 'package:archive/archive.dart';
import 'package:http/http.dart' as http;
import 'package:path/path.dart';
import 'package:csv/csv.dart';
import 'package:sqflite/sqflite.dart';
import 'package:trammy/services/gtfs_service.dart';

class GtfsDbBuilder {
  static const gtfsUrl = 'https://gtfs.sofiatraffic.bg/api/v1/static';

  late Database db;
  late String path;

  Future<void> initialize(String path) async {
    print('init()');

    this.path = path;

    db = await openDatabase(
      path,
      version: 1,
      onCreate: (db, version) async {
        // Stops table
        await db.execute('''
          CREATE TABLE agency (
            agency_id TEXT PRIMARY KEY,
            agency_name TEXT,
            agency_url TEXT,
            agency_timezone TEXT,
            agency_lang TEXT,
            agency_phone TEXT,
            agency_email TEXT
          )
          ''');

        await db.execute('''
          CREATE TABLE calendar_dates (
            service_id TEXT,
            date TEXT,
            exception_type INTEGER,
            PRIMARY KEY (service_id, date)
          )
          ''');

        await db.execute('''
          CREATE TABLE fare_attributes (
            fare_id TEXT PRIMARY KEY,
            price REAL,
            currency_type TEXT,
            payment_method INTEGER,
            transfers INTEGER,
            agency_id TEXT,
            transfer_duration INTEGER
          )
          ''');

        await db.execute('''
          CREATE TABLE feed_info (
            feed_publisher_name TEXT,
            feed_publisher_url TEXT,
            feed_lang TEXT,
            default_lang TEXT,
            feed_start_date TEXT,
            feed_end_date TEXT,
            feed_version TEXT,
            feed_contact_email TEXT,
            feed_contact_url TEXT
          )
          ''');

        await db.execute('''
          CREATE TABLE levels (
            level_id TEXT PRIMARY KEY,
            level_index REAL,
            level_name TEXT
          )
          ''');

        await db.execute('''
          CREATE TABLE pathways (
            pathway_id TEXT PRIMARY KEY,
            from_stop_id TEXT,
            to_stop_id TEXT,
            pathway_mode INTEGER,
            is_bidirectional INTEGER,
            length REAL,
            traversal_time INTEGER,
            stair_count INTEGER,
            max_slope REAL,
            min_width REAL,
            signposted_as TEXT,
            reversed_signposted_as TEXT
          )
          ''');

        await db.execute('''
          CREATE TABLE routes (
            route_id TEXT PRIMARY KEY,
            agency_id TEXT,
            route_short_name TEXT,
            route_long_name TEXT,
            route_desc TEXT,
            route_type INTEGER,
            route_url TEXT,
            route_color TEXT,
            route_text_color TEXT,
            route_sort_order INTEGER,
            continuous_pickup INTEGER,
            continuous_drop_off INTEGER
          )
          ''');

        await db.execute('''
          CREATE TABLE shapes (
            shape_id TEXT,
            shape_pt_lat REAL,
            shape_pt_lon REAL,
            shape_pt_sequence INTEGER,
            shape_dist_traveled REAL,
            PRIMARY KEY (shape_id, shape_pt_sequence)
          )
          ''');

        await db.execute('''
          CREATE TABLE stop_times (
            trip_id TEXT,
            arrival_time TEXT,
            departure_time TEXT,
            stop_id TEXT,
            stop_sequence INTEGER,
            stop_headsign TEXT,
            pickup_type INTEGER,
            drop_off_type INTEGER,
            shape_dist_traveled REAL,
            continuous_pickup INTEGER,
            continuous_drop_off INTEGER,
            timepoint INTEGER,
            PRIMARY KEY (trip_id, stop_sequence)
          )
          ''');

        await db.execute('''
          CREATE TABLE stops (
            stop_id TEXT PRIMARY KEY,
            stop_code TEXT,
            stop_name TEXT,
            stop_desc TEXT,
            stop_lat REAL,
            stop_lon REAL,
            location_type INTEGER,
            parent_station TEXT,
            stop_timezone TEXT,
            level_id TEXT
          )
          ''');

        await db.execute('''
          CREATE TABLE transfers (
            from_stop_id TEXT,
            to_stop_id TEXT,
            from_route_id TEXT,
            to_route_id TEXT,
            from_trip_id TEXT,
            to_trip_id TEXT,
            transfer_type INTEGER,
            min_transfer_time INTEGER
          )
          ''');

        await db.execute('''
          CREATE TABLE translations (
            table_name TEXT,
            field_name TEXT,
            language TEXT,
            translation TEXT,
            record_id TEXT,
            record_sub_id TEXT,
            field_value TEXT
          )
          ''');

        await db.execute('''
          CREATE TABLE trips (
            trip_id TEXT PRIMARY KEY,
            route_id TEXT,
            service_id TEXT,
            trip_headsign TEXT,
            trip_short_name TEXT,
            direction_id INTEGER,
            block_id TEXT,
            shape_id TEXT,
            wheelchair_accessible INTEGER,
            bikes_allowed INTEGER
          )
          ''');

        await db.execute(
          'CREATE INDEX IF NOT EXISTS idx_stop_times_stop_id ON stop_times(stop_id);',
        );
        await db.execute(
          'CREATE INDEX IF NOT EXISTS idx_stop_times_trip_id ON stop_times(trip_id);',
        );
        await db.execute(
          'CREATE INDEX IF NOT EXISTS idx_trips_trip_id ON trips(trip_id);',
        );
        await db.execute(
          'CREATE INDEX IF NOT EXISTS idx_trips_route_id ON trips(route_id);',
        );
        await db.execute(
          'CREATE INDEX IF NOT EXISTS idx_routes_route_id ON routes(route_id);',
        );
        await db.execute(
          'CREATE INDEX IF NOT EXISTS idx_stops_stop_code ON stops(stop_code);',
        );

        await db.execute(
          'CREATE INDEX IF NOT EXISTS idx_trips_service_id ON trips(service_id);',
        );

        await db.execute(
          'CREATE INDEX IF NOT EXISTS idx_calendar_dates_service_date '
          'ON calendar_dates(service_id, date, exception_type);',
        );
        
      },

    );
  }

  Future<void> createStopsWithRoutesTable() async {
    print('Creating stop_route_info table');
    await db?.execute('''
          CREATE TABLE IF NOT EXISTS stop_route_info AS
          SELECT 
  stops.stop_id,
  stops.stop_code,
  stops.stop_name,
  stops.stop_desc,
  stops.stop_lat,
  stops.stop_lon,
  stops.location_type,
  stops.parent_station,
  stops.stop_timezone,
  stops.level_id,
  GROUP_CONCAT(DISTINCT routes.route_type) AS route_types,
  GROUP_CONCAT(DISTINCT routes.route_color) AS route_colors,
  GROUP_CONCAT(DISTINCT routes.route_short_name) AS route_short_names
FROM stops
JOIN stop_times ON stops.stop_id = stop_times.stop_id
JOIN trips ON stop_times.trip_id = trips.trip_id
JOIN routes ON trips.route_id = routes.route_id
WHERE stops.stop_lat IS NOT NULL AND stops.stop_lon IS NOT NULL
GROUP BY stops.stop_code;
        ''');
  }

  Future<void> importFileFast({
    required String table,
    required File file,
    required void Function(GTFSProgress) onProgress,
  }) async {
    print("Importing $table from ${file.path}");
    final lines = file
        .openRead()
        .transform(utf8.decoder)
        .transform(const LineSplitter());

    final iterator = StreamIterator(lines);

    // Read header
    await iterator.moveNext();
    final header = iterator.current.split(',');

    const batchSize = 5000;
    int inserted = 0;

    List<Map<String, Object?>> batchRows = [];
    final codec = CsvCodec(dynamicTyping: true);

    while (await iterator.moveNext()) {
      final values = codec.decode(iterator.current)[0];
      final row = <String, Object?>{};
      for (int i = 0; i < header.length && i < values.length; i++) {
        final value = values[i] is String ? values[i].trim() : values[i];
        if (value is String && value.isEmpty) {
          row[header[i]] = null;
        } else {
          row[header[i]] = value;
        }
      }
      batchRows.add(row);
      inserted++;

      if (batchRows.length >= batchSize) {
        final batch = db!.batch();
        for (final r in batchRows) {
          batch.insert(table, r, conflictAlgorithm: ConflictAlgorithm.replace);
        }
        await batch.commit(noResult: true);
        batchRows.clear();
        onProgress(GTFSProgress(table: table, current: inserted));
      }
    }

    // Final batch
    if (batchRows.isNotEmpty) {
      final batch = db!.batch();
      for (final r in batchRows) {
        batch.insert(table, r, conflictAlgorithm: ConflictAlgorithm.replace);
      }
      await batch.commit(noResult: true);
      onProgress(GTFSProgress(table: table, current: inserted));
    }

    print("Finished importing $table, total rows: $inserted");
  }

  Future<void> updateGTFS({
    required void Function(GTFSProgress progress) onProgress,
    required String workingDirectory,
  }) async {
    print('UPDATING GTFS DATA');

    if (File(path).existsSync()) {
      await db.close(); // close existing DB
      await deleteDatabase(path);

      initialize(join(workingDirectory, 'gtfs.db'));

      print('Database deleted');
    }

    final zipPath = join(workingDirectory, 'gtfs.zip');
    final extractPath = join(workingDirectory, 'gtfs_extract');

    // Cleanup old
    if (Directory(extractPath).existsSync()) {
      await Directory(extractPath).delete(recursive: true);
    }
    await Directory(extractPath).create(recursive: true);

    print('Downloading GTFS ZIP from $gtfsUrl');
    // 1️⃣ Download directly to file
    final request = await http.Client().send(
      http.Request('GET', Uri.parse(gtfsUrl)),
    );
    final file = File(zipPath);
    final sink = file.openWrite();
    await request.stream.pipe(sink);
    await sink.close();

    print('Download completed, saved to $zipPath');
    // 2️⃣ Extract ZIP to disk (NOT memory)
    final bytes = await file.readAsBytes();
    final archive = ZipDecoder().decodeBytes(bytes);

    for (final entry in archive) {
      if (!entry.isFile) continue;

      final outFile = File(join(extractPath, entry.name));
      await outFile.create(recursive: true);
      await outFile.writeAsBytes(entry.content);
    }

    // 3️⃣ Process files one-by-one (memory safe)
    final fileTableMap = {
      'agency.txt': 'agency',
      'calendar_dates.txt': 'calendar_dates',
      'fare_attributes.txt': 'fare_attributes',
      'feed_info.txt': 'feed_info',
      'levels.txt': 'levels',
      'pathways.txt': 'pathways',
      'routes.txt': 'routes',
      'shapes.txt': 'shapes',
      'stop_times.txt': 'stop_times',
      'stops.txt': 'stops',
      'transfers.txt': 'transfers',
      'translations.txt': 'translations',
      'trips.txt': 'trips',
    };

    for (final entry in archive) {
      final name = entry.name.toLowerCase();
      if (!fileTableMap.containsKey(name)) continue;

      final table = fileTableMap[name]!;
      final filePath = join(extractPath, name);

      final file = File(filePath);

      await importFileFast(
        table: table,
        file: file,
        onProgress: (p) {
          onProgress(GTFSProgress(table: table, current: p.current));
        },
      );
    }

    createStopsWithRoutesTable();
  }
}
