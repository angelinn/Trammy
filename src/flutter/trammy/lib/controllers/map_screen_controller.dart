import 'package:trammy/models/gtfs/route.dart';
import 'package:trammy/models/gtfs/stop.dart';
import 'package:trammy/services/gtfs_service.dart';

class MapScreenController {
  Future<void> initialize() async {
     await GTFSService.init();
     await GTFSService.getAllStops();
  }

  Future<Map<GTFSRoute, List<DateTime>>> getUpdatesForStop(GTFSStopRouteInfo stop) async {
    await GTFSService.fetchTripUpdates();
    final tripUpdates = GTFSService.getUpdatesForStopCode(stop.stopCode!);

    Map<GTFSRoute, List<DateTime>> updates = {};
    print(tripUpdates!.length);
    for (var (tripUpdate, stopTimeUpdates) in tripUpdates!) {
      print('Processing trip update for trip ${tripUpdate.trip.tripId} at stop ${stop.stopId}');
      
    //print('Looking for route ${tripUpdate.trip.routeId} of trip ${tripUpdate.trip.tripId} at stop ${stop.stopId}');
    final route = GTFSService.routes.firstWhere((r) => r.routeId == tripUpdate.trip.routeId);
    //print('Found route ${route.routeShortName} for trip ${tripUpdate.trip.tripId}');
      final arrivals = stopTimeUpdates.map((u) => DateTime.fromMillisecondsSinceEpoch(u.arrival.time.toInt() * 1000)).toList();
  print(arrivals);

      if (updates.containsKey(route)) {
        print('Adding arrivals to existing route ${route.routeShortName}');
        updates[route]!.addAll(arrivals);
      } else {
        print('Adding new route ${route.routeShortName} with arrivals');
        updates[route] = arrivals;
      }

    }


    updates.forEach((_, times) => times.sort());

    for (var entry in updates.entries) {
      print('Route ${entry.key.routeShortName} has ${entry.value.length} arrivals: ${entry.value}');
    }
    return updates;
  }
}