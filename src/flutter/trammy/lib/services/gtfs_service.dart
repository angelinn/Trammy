import 'dart:async';
import 'dart:convert';
import 'dart:io';

import 'package:archive/archive.dart';
import 'package:csv/csv.dart';
import 'package:flutter/material.dart';
import 'package:gtfs_realtime_bindings/gtfs_realtime_bindings.dart';
import 'package:http/http.dart' as http;
import 'package:latlong2/latlong.dart';
import 'package:path/path.dart';
import 'package:path_provider/path_provider.dart';
import 'package:sqflite/sqflite.dart';
import 'package:trammy/db/gtfs_repository.dart';
import 'package:trammy/models/gtfs/route.dart';
import 'package:trammy/models/gtfs/stop.dart';
import 'package:trammy/models/gtfs/trip.dart';

class GTFSProgress {
  final String table;
  final int current;

  GTFSProgress({required this.table, required this.current});

  //double get percent => total == 0 ? 0 : current / total;
}

class GTFSService {
  static const _url = 'https://gtfs.sofiatraffic.bg/api/v1/trip-updates';
  static Map<String, GTFSStopRouteInfo> stopsById = {};
  static Map<String, List<GTFSStopRouteInfo>> stopsByCodeMap = {};

  /// stop_id -> list of updates
  static final Map<String, List<(TripUpdate, List<TripUpdate_StopTimeUpdate>)>> _updatesByStop = {};

  static DateTime? lastUpdated;

  /// Fetch and parse trip updates
  static Future<void> fetchTripUpdates() async {
    if (lastUpdated != null &&
        DateTime.now().difference(lastUpdated!) < const Duration(seconds: 60)) {
      return;
    }

    final response = await http.get(Uri.parse(_url));

    if (response.statusCode != 200) {
      throw Exception('Failed to fetch GTFS-RT');
    }

    final bytes = response.bodyBytes;

    final feed = FeedMessage.fromBuffer(bytes);

    _updatesByStop.clear();

    for (final entity in feed.entity) {
      if (!entity.hasTripUpdate()) continue;

      final tripUpdate = entity.tripUpdate;
      for (final stopTimeUpdate in tripUpdate.stopTimeUpdate) {
        final stopId = stopTimeUpdate.stopId;

        _updatesByStop.putIfAbsent(stopId, () => []);
        _updatesByStop[stopId]!.add((tripUpdate, []));
        _updatesByStop[stopId]!.last.$2.addAll(tripUpdate.stopTimeUpdate.where((u) => u.stopId == stopId).toList());
      }
    }

    print('Fetched ${feed.entity.length} trip updates for ${_updatesByStop.length} stops');
    lastUpdated = DateTime.now();
  }

  /// Query by stop_id directly
  static List<(TripUpdate, List<TripUpdate_StopTimeUpdate>)>? getUpdatesForStopId(String stopId) {
    return _updatesByStop[stopId];
  }

  /// Query by stopCode (requires stop lookup map)
  static List<(TripUpdate, List<TripUpdate_StopTimeUpdate>)>? getUpdatesForStopCode(
    String stopCode
  ) {
    final stops = stopsByCodeMap[stopCode];

    if (stops == null || stops.isEmpty) {
      return null;
    }

    List<(TripUpdate, List<TripUpdate_StopTimeUpdate>)> allUpdates = [];
    for (var stop in stops) {
      allUpdates.addAll(getUpdatesForStopId(stop.stopId) ?? []);
    }

    return allUpdates;
  }

  static List<GTFSStopRouteInfo> stops = [];
  static List<GTFSStopRouteInfo> stopsByCode = [];
  static List<GTFSRoute> routes = [];
  static Map<String, GTFSTrip> trips = {};
  static final GTFSRepository repo = GTFSRepository();


  /// Initialize database
  static Future<void> init() async {
    print('[GTFSService] init()');

    final path = join(
      (await getApplicationDocumentsDirectory()).path,
      'gtfs.db',
    );

    await repo.initialize(path);
  }


  static Future<void> updateGTFS({
    required void Function(GTFSProgress progress) onProgress,
  }) async {
    print('[GTFSService] updateGTFS()');
    await repo.updateGTFS(
      onProgress: onProgress,
      workingDirectory: (await getApplicationDocumentsDirectory()).path,
    );
  }

  /// Example query: get all stops
  static Future<List<GTFSStopRouteInfo>> getAllStops() async {
    var dbStops = await repo?.getStops();
    stops = dbStops!
        .where((stop) => stop.stopCode != null && stop.stopCode != '')
        .toList();

    for (final stop in stops) {
      stopsById[stop.stopId] = stop;
      stopsByCodeMap.putIfAbsent(stop.stopCode!, () => []).add(stop);
    }

    stopsByCode = stopsByCodeMap.values.map((v) => v.first).toList();

    routes = (await repo?.getRoutes())!;
    final dbTrips = await repo.getTrips();
    for (final trip in dbTrips) {
      trips[trip.tripId] = trip;
    }

    print(
      'Loaded ${stops.length} stops and ${routes.length} routes from database',
    );

    return stopsByCode;
  }

  static IconData getStopIcon(int dominantType) {
    switch (dominantType) {
      case 0:
        return Icons.tram;
      case 3:
        return Icons.directions_bus;
      case 11:
        return Icons.electric_scooter;
      default:
        return Icons.directions_bus;
    }
  }

  static GTFSRoute findByTripId(String tripId) {
    return routes.firstWhere(
      (r) => r.routeId == tripId,
      orElse: () => GTFSRoute(routeId: tripId),
    );
  } 

  static GTFSStopRouteInfo? findNearestStop(LatLng coords) {
    const double maxDistanceMeters = 20;

    GTFSStopRouteInfo? closest;
    double minDistance = double.infinity;

    for (final stop in GTFSService.stopsByCode) {
      if (stop.stopLat == null || stop.stopLon == null) continue;
      final distance = const Distance().as(
        LengthUnit.Meter,
        coords,
        LatLng(stop.stopLat!, stop.stopLon!),
      );

      if (distance < minDistance && distance < maxDistanceMeters) {
        minDistance = distance;
        closest = stop;
      }
    }
     if (closest != null) {
      print('Tapped near stop: ${closest.stopName} (${closest.stopId}), distance: ${minDistance.toStringAsFixed(1)}m');

      return closest;
    } 

    print('Tapped at ${coords.latitude.toStringAsFixed(5)}, ${coords.longitude.toStringAsFixed(5)}, no nearby stop (closest is ${minDistance.toStringAsFixed(1)}m away)',);
    return null;
  }

  static List<GTFSStopRouteInfo> searchStop(String query) {
    return stopsByCode.where(
      (s) =>
          s.stopName != null && s.stopCode != null &&
          "${s.stopName!.toLowerCase()} (${s.stopCode!.toLowerCase()})" == query,
    ).toList();
  }
}
