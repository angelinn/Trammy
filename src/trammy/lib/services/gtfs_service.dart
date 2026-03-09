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
import 'package:trammy/models/gtfs/shape.dart';
import 'package:trammy/models/gtfs/stop.dart';
import 'package:trammy/models/gtfs/stop_time.dart';
import 'package:trammy/models/gtfs/trip.dart';
import 'package:trammy/services/city_exceptions.dart';
import 'package:trammy/services/common.dart';

class GTFSProgress {
  final String table;
  final int current;

  GTFSProgress({required this.table, required this.current});

  //double get percent => total == 0 ? 0 : current / total;
}

class GTFSService {
  static const TRIP_UPDATES_URL = 'https://gtfs.sofiatraffic.bg/api/v1/trip-updates';
  static const VEHICLE_POSITIONS_URL = 'https://gtfs.sofiatraffic.bg/api/v1/vehicle-positions';
  static Map<String, String> stopIdToCode = {};
  static Map<String, List<String>> stopCodeToId = {};
  static Map<String, List<GTFSStopRouteInfo>> stopsByCodeMap = {};

  static List<GTFSStopRouteInfo> stops = [];
  static List<GTFSStopRouteInfo> stopsByCode = [];
  static List<GTFSRoute> routes = [];
  static Map<String, GTFSTrip> trips = {};
  static final GTFSRepository repo = GTFSRepository();
  static final Map<String, DateTime> predictedArrivals = {};

  static Map<TransportType, String> routeColors = {
    TransportType.tram: 'F7941D',
    TransportType.bus: 'BE1E2D',
    TransportType.trolley: '27AAE1',
    TransportType.subway: '39B54A',
    TransportType.night: '000000'
  };

  static DateTime? lastUpdated;

  /// Fetch and parse trip updates
  static Future<void> fetchTripUpdates() async {
    if (lastUpdated != null &&
        DateTime.now().difference(lastUpdated!) < const Duration(seconds: 60)) {
      return;
    }

    final response = await http.get(Uri.parse(TRIP_UPDATES_URL));

    if (response.statusCode != 200) {
      throw Exception('Failed to fetch GTFS-RT');
    }

    final bytes = response.bodyBytes;

    final feed = FeedMessage.fromBuffer(bytes);

    predictedArrivals.clear();

    for (final entity in feed.entity) {
      if (!entity.hasTripUpdate()) continue;

      final tripUpdate = entity.tripUpdate;
      for (final stopTimeUpdate in tripUpdate.stopTimeUpdate) {
        final key = "${tripUpdate.trip.tripId}_${stopTimeUpdate.stopId}";
        predictedArrivals.putIfAbsent(key, () => fromUnixTime(stopTimeUpdate.arrival.time.toInt()));
      }
    }

    print('Fetched ${feed.entity.length} trip updates for ${predictedArrivals.length} stops');
    lastUpdated = DateTime.now();
  } 

  static final ValueNotifier<Map<String, List<VehiclePosition>>> vehiclesNotifier= ValueNotifier({});
  static Map<String, List<VehiclePosition>> vehiclesPositions = {};

  static DateTime? lastUpdatedVehicles;
  static Timer? _vehicleTimer;

  static void startVehicleUpdates() {
    if (_vehicleTimer != null) return;
    // Run immediately, then every 30 sec
    _vehicleTimer = Timer.periodic(const Duration(seconds: 10), (_) async {
      await fetchVehiclePositions();
    });      
    
    fetchVehiclePositions();
  }

  static void stopVehicleUpdates() {
    _vehicleTimer?.cancel();
    vehiclesNotifier.value = {};
  }

  static Future<void> fetchVehiclePositions() async {
       if (lastUpdatedVehicles != null &&
        DateTime.now().difference(lastUpdatedVehicles!) < const Duration(seconds: 30)) {
      return;
    }

    vehiclesPositions.clear();
    vehiclesPositions = {};

    final response = await http.get(Uri.parse(VEHICLE_POSITIONS_URL));

    if (response.statusCode != 200) {
      throw Exception('Failed to fetch GTFS-RT');
    }

    final bytes = response.bodyBytes;

    final feed = FeedMessage.fromBuffer(bytes); 
    for (final entity in feed.entity) {
      if (!entity.hasVehicle()) continue;
      final vehicle = entity.vehicle;

      print('Adding vehicle ${vehicle.trip.routeId} for trip ${vehicle.trip.tripId} with id ${vehicle.vehicle.id}');
      vehiclesPositions.putIfAbsent(vehicle.trip.routeId, () => []).add(vehicle);
    }

    vehiclesNotifier.value = vehiclesPositions;
  }

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

    var idsToCode = await repo.getStopsRaw(['stop_id', 'stop_code']);
    for (final idToCode in idsToCode) {
      if (idToCode.stopCode == null || idToCode.stopCode == '') continue;
      stopIdToCode[idToCode.stopId] = idToCode.stopCode!;
      stopCodeToId.putIfAbsent(idToCode.stopCode!, () => []).add(idToCode.stopId);
    }


    for (final stop in stops) {
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

  static IconData getStopIcon(TransportType dominantType) {
    switch (dominantType) {
      case TransportType.tram:
        return Icons.tram;
      case TransportType.bus:
        return Icons.directions_bus;
      case TransportType.trolley:
        return Icons.directions_bus;
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

  static Future<Map<String, List<GTFSStopTimeData>>> getStopTimesAfterNow(String stopId, int limit) async {
    final departures = await repo.getStopTimesAfterNow(stopId, limit);

    Map<String, List<GTFSStopTimeData>> departuresMap = {};
    for (final dep in departures) {
      departuresMap.putIfAbsent(dep.routeId!, () => []).add(dep);
      }

      return departuresMap;
  }

  static TransportType? getDominantType(GTFSStopRouteInfo stop) {
    if (stop.routeTypes == null || stop.routeTypes!.isEmpty) return null;

    final routeIds = stop.routeIds!.split(','); 
    List<TransportType> routeTypes = [];
    for (final routeId in routeIds) {
      final route = routes.firstWhere((r) => r.routeId == routeId);
      final type = SofiaExceptions.getRealType(route);

      routeTypes.add(type!);
    }

    if (routeTypes.contains(TransportType.tram)) return TransportType.tram;
    if (routeTypes.contains(TransportType.trolley)) return TransportType.trolley;
    if (routeTypes.contains(TransportType.bus)) return TransportType.bus;
    if (routeTypes.contains(TransportType.subway)) return TransportType.subway;

    return null;
  }

  static String? getDominantColor(GTFSStopRouteInfo stop) {
    TransportType? routeType = getDominantType(stop);
    return routeColors[TransportType.fromValue(routeType!.value)];
  }

  static String? getExceptionColor(GTFSRoute route) {      
    final type = SofiaExceptions.getRealType(route);
    return routeColors[TransportType.fromValue(type!.value)];
  }

  static Future<List<GTFSShape>> getShapes(String shapeId) async {
    final shapes = await repo.getShapes(shapeId);
    print('Loaded ${shapes.length} shapes for $shapeId');

    return shapes;
  }
}
