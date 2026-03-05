import 'package:gtfs_realtime_bindings/gtfs_realtime_bindings.dart';
import 'package:trammy/models/gtfs/route.dart';
import 'package:trammy/models/gtfs/stop.dart';
import 'package:trammy/models/gtfs/stop_time.dart';
import 'package:trammy/services/gtfs_service.dart';

  class StopInfoKey { 
    final GTFSRoute route;
    final String direction;

  StopInfoKey(this.route, this.direction);

    @override
  bool operator ==(Object other) {
    if (identical(this, other)) return true;
    return other is StopInfoKey &&
        other.route == route &&
        other.direction == direction;
  }

  @override
  int get hashCode => Object.hash(route, direction);
  }

class ArrivalEntry {
  final DateTime arrival;
  final bool online;

  ArrivalEntry(this.arrival, this.online);
}

class MapScreenController {
  Future<void> initialize() async {
     await GTFSService.init();
     await GTFSService.getAllStops();
  }

  Future<Map<StopInfoKey, List<ArrivalEntry>>> getUpdatesForStop(GTFSStopRouteInfo stop) async {
    Map<StopInfoKey, List<ArrivalEntry>> updates = {};

    await GTFSService.fetchTripUpdates();
    Map<String, List<GTFSStopTimeData>> stopTimes = await GTFSService.getStopTimesAfterNow(stop.stopCode!, 3);

    for (GTFSStopTimeData stopTime in stopTimes.values.expand((v) => v)) {
      final route = GTFSService.routes.firstWhere((r) => r.routeId == stopTime.routeId);
      final predictedArrival = GTFSService.predictedArrivals["${stopTime.tripId}_${stopTime.stopId}"];
      if (predictedArrival != null) {
        updates.putIfAbsent(StopInfoKey(route, stopTime.tripHeadsign!), () => []).add(ArrivalEntry(predictedArrival, true));
      }
      else {
        updates.putIfAbsent(StopInfoKey(route, stopTime.tripHeadsign!), () => []).add(ArrivalEntry(stopTime.toArrivalDateTime(), false));
      }
    }   

    updates.forEach((_, times) => times.sort((a, b) => a.arrival.compareTo(b.arrival)));

    for (var entry in updates.entries) {
      print('Route ${entry.key.route.routeShortName} has ${entry.value.length} arrivals: ${entry.value}');
    }
    return updates;
  }
}