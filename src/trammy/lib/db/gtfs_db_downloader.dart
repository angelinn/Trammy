import 'dart:async';
import 'dart:convert';
import 'dart:io';

import 'package:archive/archive_io.dart';
import 'package:csv/csv.dart';
import 'package:http/http.dart' as http;
import 'package:path/path.dart';
import 'package:shared_preferences/shared_preferences.dart';
import 'package:sqflite/sqflite.dart';
import 'package:trammy/services/gtfs_service.dart';
import 'package:zstd/zstd.dart';

class GtfsDbDownloader {

  Database? _db;
  final String path;
  GtfsDbDownloader(this.path);

  static const String dbDownloadUrl =
      'https://github.com/angelinn/Trammy/releases/download/latest/sofia.db.zst';
  static const String hashDownloadUrl =
      'https://github.com/angelinn/Trammy/releases/download/latest/gtfs.hash';

  static const String _hashKey = 'gtfs_db_hash';

  Database get db => _db!;

  Future<void> initialize() async {
    final dbExists = await File(path).exists();
    if (!dbExists || _db != null) return;

    _db = await openDatabase(
      path,
      version: 1
    );

    // SQLite performance settings
    await _db!.rawQuery('PRAGMA synchronous = OFF');
    await _db!.rawQuery('PRAGMA journal_mode = MEMORY');
    await _db!.rawQuery('PRAGMA temp_store = MEMORY');
    await _db!.rawQuery('PRAGMA cache_size = -64000');
  }

  Future<void> updateGTFS({
    required void Function(GTFSProgress progress) onProgress,
    required String workingDirectory,
  }) async {
    onProgress(GTFSProgress(table: 'Проверка за нови разписания', current: 0));

    final hashResponse = await http.get(Uri.parse(hashDownloadUrl));
    if (hashResponse.statusCode != 200) {
      throw Exception('Failed to download GTFS hash: ${hashResponse.statusCode}');
    }
    final remoteHash = hashResponse.body.trim();

    final prefs = await SharedPreferences.getInstance();
    final storedHash = prefs.getString(_hashKey);

    if (storedHash == remoteHash && File(path).existsSync()) {
      print('GTFS database is up to date (hash: $remoteHash)');
      onProgress(GTFSProgress(table: 'Up to date', current: 1));
      return;
    }

    print('GTFS database outdated. Remote: $remoteHash, Local: $storedHash');

    final compressedPath = join(workingDirectory, 'sofia.db.zst');
    final tempDbPath = join(workingDirectory, 'sofia_new.db');

    onProgress(GTFSProgress(table: 'Изтегляне', current: 0));

    final client = http.Client();
    try {
      final response = await client.send(http.Request('GET', Uri.parse(dbDownloadUrl)));
      final sink = File(compressedPath).openWrite();

      int downloadedBytes = 0;
      await for (final chunk in response.stream) {
        sink.add(chunk);
        downloadedBytes += chunk.length;
        onProgress(GTFSProgress(table: 'Изтегляне', current: downloadedBytes));
      }
      await sink.close();
    } finally {
      client.close();
    }

    onProgress(GTFSProgress(table: 'Разархивиране', current: 0));

    final bytes = await File(compressedPath).readAsBytes();
    final decompressed = zstd.decode(bytes);
    await File(tempDbPath).writeAsBytes(decompressed);
    await File(compressedPath).delete();

    if (_db != null) {
      await _db!.close();
      _db = null;
    }

    if (await File(path).exists()) {
      await deleteDatabase(path);
    }
    await File(tempDbPath).rename(path);

    await initialize();

    await prefs.setString(_hashKey, remoteHash);

    onProgress(GTFSProgress(table: 'Done', current: 1));
  }
}