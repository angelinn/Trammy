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

  late Database db;
  final String path;
  GtfsDbDownloader(this.path);

  static const String dbDownloadUrl =
      'https://github.com/angelinn/Trammy/releases/download/latest/sofia.db.zst';
  static const String hashDownloadUrl =
      'https://github.com/angelinn/Trammy/releases/download/latest/gtfs.hash';

  static const String _hashKey = 'gtfs_db_hash';

  Future<void> initialize() async {
    final dbExists = await File(path).exists();
    if (!dbExists) return;

    db = await openDatabase(
      path,
      version: 1
    );

    // SQLite performance settings
    await db.rawQuery('PRAGMA synchronous = OFF');
    await db.rawQuery('PRAGMA journal_mode = MEMORY');
    await db.rawQuery('PRAGMA temp_store = MEMORY');
    await db.rawQuery('PRAGMA cache_size = -64000');
  }

  Future<void> updateGTFS({
    required void Function(GTFSProgress progress) onProgress,
    required String workingDirectory,
  }) async {
    onProgress(GTFSProgress(table: 'Checking for updates', current: 0));

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

    // Close existing database if open
    if (File(path).existsSync()) {
      //await db.close();
      await deleteDatabase(path);
    }

    final dbPath = join(workingDirectory, 'sofia.db.zst');

    onProgress(GTFSProgress(table: 'Downloading database', current: 0));

    // Download the compressed database
    final request = await http.Client().send(http.Request('GET', Uri.parse(dbDownloadUrl)));
    final response = request;

    final zipFile = File(dbPath);
    final sink = zipFile.openWrite();

    int downloadedBytes = 0;
    await for (final chunk in response.stream) {
      sink.add(chunk);
      downloadedBytes += chunk.length;
      onProgress(GTFSProgress(table: 'Downloading', current: downloadedBytes));
    }
    await sink.close();

    onProgress(GTFSProgress(table: 'Decompressing', current: 0));

    // Decompress zstd
    final zstFile = File(dbPath);
    final bytes = zstFile.readAsBytesSync();
    final decompressed = zstd.decode(bytes);

    // Write decompressed database
    final outputFile = File(path);
    await outputFile.writeAsBytes(decompressed);

    // Clean up compressed file
    await zstFile.delete();

    await initialize();

    // Save new hash
    await prefs.setString(_hashKey, remoteHash);

    onProgress(GTFSProgress(table: 'Done', current: 1));
  }
}