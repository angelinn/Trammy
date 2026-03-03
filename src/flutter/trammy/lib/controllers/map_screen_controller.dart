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
    final tripUpdates = GTFSService.getUpdatesForStopId(stop.stopId);

    Map<GTFSRoute, List<DateTime>> updates = {};
    for (final tripUpdate in tripUpdates) {
      print('Looking for route ${tripUpdate.trip.routeId} of trip ${tripUpdate.trip.tripId} at stop ${stop.stopId}');
      final route = GTFSService.routes.firstWhere((r) => r.routeId == tripUpdate.trip.routeId);
      print('Found route ${route.routeShortName} for trip ${tripUpdate.trip.tripId}');

      final arrivals = tripUpdate.stopTimeUpdate.map((u) => fromUnix(u.arrival.time.toInt()))
        .toList();

      if (updates.containsKey(route)) {
        updates[route]!.addAll(arrivals);
      } else {
        updates[route] = arrivals;
      }
    }

    updates.forEach((_, times) => times.sort());
    return updates;
  }

    DateTime fromUnix(int seconds) {
    return DateTime.fromMillisecondsSinceEpoch(seconds * 1000);
  }
}