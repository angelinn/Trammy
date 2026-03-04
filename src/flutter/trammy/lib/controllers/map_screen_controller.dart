import 'package:trammy/models/gtfs/route.dart';
import 'package:trammy/models/gtfs/stop.dart';
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


class MapScreenController {
  Future<void> initialize() async {
     await GTFSService.init();
     await GTFSService.getAllStops();
  }

  Future<Map<StopInfoKey, List<DateTime>>> getUpdatesForStop(GTFSStopRouteInfo stop) async {
    await GTFSService.fetchTripUpdates();
    final tripUpdates = GTFSService.getUpdatesForStopCode(stop.stopCode!);

    Map<StopInfoKey, List<DateTime>> updates = {};
    print(tripUpdates!.length);
    for (var (tripUpdate, stopTimeUpdates) in tripUpdates!) {
      print('Processing trip update for trip ${tripUpdate.trip.tripId} at stop ${stop.stopId}');
      
    //print('Looking for route ${tripUpdate.trip.routeId} of trip ${tripUpdate.trip.tripId} at stop ${stop.stopId}');
    final route = GTFSService.routes.firstWhere((r) => r.routeId == tripUpdate.trip.routeId);
    final tripHeadsign = GTFSService.trips[tripUpdate.trip.tripId]?.headsign ?? 'Unknown direction';

    //print('Found route ${route.routeShortName} for trip ${tripUpdate.trip.tripId}');
    final key = StopInfoKey(route, tripHeadsign);
      final arrivals = stopTimeUpdates.map((u) => DateTime.fromMillisecondsSinceEpoch(u.arrival.time.toInt() * 1000)).toList();
  print(arrivals);

      if (updates.containsKey(key)) {
        print('Adding arrivals to existing route ${route.routeShortName}');
        updates[key]!.addAll(arrivals);
      } else {
        print('Adding new route ${route.routeShortName} with arrivals');
        updates[key] = arrivals;
      }

    }


    updates.forEach((_, times) => times.sort());

    for (var entry in updates.entries) {
      print('Route ${entry.key.route.routeShortName} has ${entry.value.length} arrivals: ${entry.value}');
    }
    return updates;
  }
}