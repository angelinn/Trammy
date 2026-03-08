import 'dart:async';
import 'dart:convert';
import 'dart:io';

import 'package:archive/archive_io.dart';
import 'package:csv/csv.dart';
import 'package:http/http.dart' as http;
import 'package:path/path.dart';
import 'package:sqflite/sqflite.dart';
import 'package:trammy/services/gtfs_service.dart';

class GtfsDbBuilder {
  static const gtfsUrl = 'https://gtfs.sofiatraffic.bg/api/v1/static';

  late Database db;
  late String path;

  Future<void> initialize(String path) async {
    this.path = path;

    db = await openDatabase(
      path,
      version: 1,
      onCreate: (db, version) async {
        await _createTables(db);
      },
    );

    // SQLite performance settings
    await db.rawQuery('PRAGMA synchronous = OFF');
    await db.rawQuery('PRAGMA journal_mode = MEMORY');
    await db.rawQuery('PRAGMA temp_store = MEMORY');
    await db.rawQuery('PRAGMA cache_size = -64000');
  }

  Future<void> _createTables(Database db) async {
    await db.execute('''
CREATE TABLE agency(
agency_id TEXT PRIMARY KEY,
agency_name TEXT,
agency_url TEXT,
agency_timezone TEXT,
agency_lang TEXT,
agency_phone TEXT,
agency_email TEXT)
''');

    await db.execute('''
CREATE TABLE calendar_dates(
service_id TEXT,
date TEXT,
exception_type INTEGER,
PRIMARY KEY(service_id,date))
''');

    await db.execute('''
CREATE TABLE routes(
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
continuous_drop_off INTEGER)
''');

    await db.execute('''
CREATE TABLE trips(
trip_id TEXT PRIMARY KEY,
route_id TEXT,
service_id TEXT,
trip_headsign TEXT,
trip_short_name TEXT,
direction_id INTEGER,
block_id TEXT,
shape_id TEXT,
wheelchair_accessible INTEGER,
bikes_allowed INTEGER)
''');

    await db.execute('''
CREATE TABLE stops(
stop_id TEXT PRIMARY KEY,
stop_code TEXT,
stop_name TEXT,
stop_desc TEXT,
stop_lat REAL,
stop_lon REAL,
location_type INTEGER,
parent_station TEXT,
stop_timezone TEXT,
level_id TEXT)
''');

    await db.execute('''
CREATE TABLE stop_times(
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
PRIMARY KEY(trip_id,stop_sequence))
''');

    await db.execute('''
CREATE TABLE shapes(
shape_id TEXT,
shape_pt_lat REAL,
shape_pt_lon REAL,
shape_pt_sequence INTEGER,
shape_dist_traveled REAL,
PRIMARY KEY(shape_id,shape_pt_sequence))
''');

    await db.execute('''
CREATE TABLE transfers(
from_stop_id TEXT,
to_stop_id TEXT,
from_route_id TEXT,
to_route_id TEXT,
from_trip_id TEXT,
to_trip_id TEXT,
transfer_type INTEGER,
min_transfer_time INTEGER)
''');

    await db.execute('''
CREATE TABLE translations(
table_name TEXT,
field_name TEXT,
language TEXT,
translation TEXT,
record_id TEXT,
record_sub_id TEXT,
field_value TEXT)
''');

    await db.execute('''
CREATE TABLE fare_attributes(
fare_id TEXT PRIMARY KEY,
price REAL,
currency_type TEXT,
payment_method INTEGER,
transfers INTEGER,
agency_id TEXT,
transfer_duration INTEGER)
''');

    await db.execute('''
CREATE TABLE feed_info(
feed_publisher_name TEXT,
feed_publisher_url TEXT,
feed_lang TEXT,
default_lang TEXT,
feed_start_date TEXT,
feed_end_date TEXT,
feed_version TEXT,
feed_contact_email TEXT,
feed_contact_url TEXT)
''');

    await db.execute('''
CREATE TABLE levels(
level_id TEXT PRIMARY KEY,
level_index REAL,
level_name TEXT)
''');

    await db.execute('''
CREATE TABLE pathways(
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
reversed_signposted_as TEXT)
''');
  }

  Future<void> _createIndexes() async {
    await db.execute(
        'CREATE INDEX idx_stop_times_stop_id ON stop_times(stop_id)');
    await db.execute(
        'CREATE INDEX idx_stop_times_trip_id ON stop_times(trip_id)');
    await db.execute('CREATE INDEX idx_trips_trip_id ON trips(trip_id)');
    await db.execute('CREATE INDEX idx_trips_route_id ON trips(route_id)');
    await db.execute('CREATE INDEX idx_routes_route_id ON routes(route_id)');
    await db.execute('CREATE INDEX idx_stops_stop_code ON stops(stop_code)');
    await db.execute('CREATE INDEX idx_trips_service_id ON trips(service_id)');
    await db.execute(
        'CREATE INDEX idx_calendar_dates_service_date ON calendar_dates(service_id,date,exception_type)');
  }


  Future<void> createStopsWithRoutesTable() async {
    print('Creating stop_route_info table');
    await db?.execute('''
          CREATE TABLE IF NOT EXISTS stop_route_info AS
          SELECT 
  stops.stop_id,
  CAST(stops.stop_code AS TEXT) AS stop_code,
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
  GROUP_CONCAT(DISTINCT routes.route_id) AS route_ids
FROM stops
JOIN stop_times ON stops.stop_id = stop_times.stop_id
JOIN trips ON stop_times.trip_id = trips.trip_id
JOIN routes ON trips.route_id = routes.route_id
WHERE stops.stop_lat IS NOT NULL AND stops.stop_lon IS NOT NULL
GROUP BY stops.stop_code;
        ''');
  }


  Future<void> importFileFast({
    required DatabaseExecutor executor,
    required String table,
    required File file,
    required void Function(GTFSProgress) onProgress,
  }) async {
    final codec = CsvCodec();
    const batchSize = 20000;

    final lines = file
        .openRead()
        .transform(utf8.decoder)
        .transform(const LineSplitter());

    final iterator = StreamIterator(lines);

    await iterator.moveNext();
    final header = codec.decode(iterator.current)[0];

    final placeholders =
        List.filled(header.length, '?').join(',');

    final sql =
        'INSERT OR REPLACE INTO $table (${header.join(",")}) VALUES ($placeholders)';

    int inserted = 0;
    List<List<dynamic>> batchRows = [];

    while (await iterator.moveNext()) {
      final values = codec.decode(iterator.current)[0];

      for (int i = 0; i < values.length; i++) {
        if (values[i] is String && (values[i] as String).isEmpty) {
          values[i] = null;
        }
      }

      batchRows.add(values);
      inserted++;

      if (batchRows.length >= batchSize) {
        final batch = executor.batch();

        for (final row in batchRows) {
          batch.rawInsert(sql, row);
        }

        await batch.commit(noResult: true);
        batchRows.clear();

        onProgress(GTFSProgress(table: table, current: inserted));
      }
    }

    if (batchRows.isNotEmpty) {
      final batch = executor.batch();

      for (final row in batchRows) {
        batch.rawInsert(sql, row);
      }

      await batch.commit(noResult: true);

      onProgress(GTFSProgress(table: table, current: inserted));
    }
  }

  Future<void> updateGTFS({
    required void Function(GTFSProgress progress) onProgress,
    required String workingDirectory,
  }) async {
    if (File(path).existsSync()) {
      await db.close();
      await deleteDatabase(path);
      await initialize(path);
    }

    final zipPath = join(workingDirectory, 'gtfs.zip');
    final extractPath = join(workingDirectory, 'gtfs_extract');

    if (Directory(extractPath).existsSync()) {
      await Directory(extractPath).delete(recursive: true);
    }
    await Directory(extractPath).create(recursive: true);

    final request =
        await http.Client().send(http.Request('GET', Uri.parse(gtfsUrl)));

    final zipFile = File(zipPath);
    final sink = zipFile.openWrite();

    await request.stream.pipe(sink);
    await sink.close();

    final inputStream = InputFileStream(zipPath);
    final archive = ZipDecoder().decodeStream(inputStream);

    for (final entry in archive) {
      if (!entry.isFile) continue;

      final outFile = File(join(extractPath, entry.name));
      await outFile.create(recursive: true);

      final output = OutputFileStream(outFile.path);
      entry.writeContent(output);
      await output.close();
    }

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

    await db.transaction((txn) async {
      for (final entry in archive) {
        final name = entry.name.toLowerCase();
        if (!fileTableMap.containsKey(name)) continue;

        final table = fileTableMap[name]!;
        final filePath = join(extractPath, name);

        await importFileFast(
          executor: txn,
          table: table,
          file: File(filePath),
          onProgress: onProgress,
        );
      }
    });

    await _createIndexes();
    await createStopsWithRoutesTable();
  }
}