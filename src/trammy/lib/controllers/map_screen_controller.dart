import 'package:trammy/models/gtfs/route.dart';
import 'package:trammy/models/gtfs/stop.dart';
import 'package:trammy/models/gtfs/stop_time.dart';
import 'package:trammy/models/gtfs/trip.dart';
import 'package:trammy/services/gtfs_service.dart';

class StopInfoKey { 
  final GTFSRoute route;
  final GTFSTrip trip;
  final String direction;

  StopInfoKey(this.route, this.trip, this.direction);

    @override
  bool operator ==(Object other) {
    if (identical(this, other)) return true;
    return other is StopInfoKey &&
        other.route.routeId == route.routeId &&
        other.trip.headsign == trip.headsign &&
        other.direction == direction;
  }

  @override
  int get hashCode => Object.hash(route, trip.headsign);
  }

class ArrivalEntry {
  final DateTime arrival;
  final bool online;

  ArrivalEntry(this.arrival, this.online);
}

class MapScreenController {
  static Future<void> initialize() async {
     await GTFSService.init();
     await GTFSService.getAllStops();
  }

  static Future<Map<StopInfoKey, List<ArrivalEntry>>> getUpdatesForStop(String searchStopCode) async {
    Map<StopInfoKey, List<ArrivalEntry>> updates = {};
    Set<String> addedKeys = {};

    await GTFSService.fetchTripUpdates();
     // 1️⃣ First pass: use GTFS-RT as truth
    GTFSService.predictedArrivals.forEach((key, predictedArrival) {
      final parts = key.split("_");
      final tripId = parts[0];
      final stopId = parts[1];

      final stopCode = GTFSService.stopIdToCode[stopId];
      if (stopCode == null) {
        print("Stop $stopId not found");
        return;
      }

      if (stopCode != searchStopCode) return;

      final trip = GTFSService.trips[tripId];
      if (trip == null) return;

      final route = GTFSService.routes.firstWhere((r) => r.routeId == trip.routeId);

      updates
          .putIfAbsent(StopInfoKey(route, trip, trip.headsign!), () => [])
          .add(ArrivalEntry(predictedArrival, true));

      addedKeys.add(key);
    });


    Map<String, List<GTFSStopTimeData>> stopTimes = await GTFSService.getStopTimesAfterNow(searchStopCode, 3);

    for (GTFSStopTimeData stopTime in stopTimes.values.expand((v) => v)) {
      final key = "${stopTime.tripId}_${stopTime.stopId}";
    if (addedKeys.contains(key)) continue;

      final route = GTFSService.routes.firstWhere((r) => r.routeId == stopTime.routeId);      
    
      updates.putIfAbsent(StopInfoKey(route, GTFSService.trips[stopTime.tripId!]!, stopTime.tripHeadsign!), () => []).add(ArrivalEntry(stopTime.toArrivalDateTime(), false));
    }   

    updates.forEach((_, times) => times.sort((a, b) => a.arrival.compareTo(b.arrival)));

    for (var entry in updates.entries) {
      print('Route ${entry.key.route.routeShortName} has ${entry.value.length} arrivals: ${entry.value}');
    }
    return updates;
  }
}