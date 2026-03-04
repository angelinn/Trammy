import 'package:flutter/material.dart';
import 'package:flutter_map_animations/flutter_map_animations.dart';
import 'package:latlong2/latlong.dart';
import 'package:trammy/models/gtfs/stop.dart';
import 'package:trammy/screens/map/widgets/animated_stop_marker.dart';
import 'package:trammy/services/common.dart';

class StopsLayer extends StatelessWidget {
  final AnimatedMapController animatedMapController;
  final List<GTFSStopRouteInfo> stops;
  final void Function(GTFSStopRouteInfo) onStopTapped;

  StopsLayer({super.key, required this.animatedMapController, required this.stops, required this.onStopTapped});


  @override
  Widget build(BuildContext context) {
    final Set<String> stopLocations = {};

    return AnimatedMarkerLayer(
      markers: stops
          .where((s) => s.stopLat != null && s.stopLon != null && isStopInView(s, stopLocations))
          .map((stop) {
            final radius = calculateRadius();

            return AnimatedMarker(
              point: LatLng(stop.stopLat!, stop.stopLon!),
              width: radius,
              height: radius,
              builder: (context, animation) {
                return AnimatedStopMarker(
                  radius: radius,
                  fillColor: colorFromHex(stop.getDominantColor()!).withOpacity(0.8),
                  borderColor: colorFromHex(stop.getDominantType()!.toString()),
                  onTap: () => onStopTapped(stop),
                );
              },
            );
          })
          .toList(),
    );
  }

 bool isStopInView(GTFSStopRouteInfo stop, Set<String> stopLocations) {
    if (animatedMapController.mapController.camera.zoom < 14) return false;

    if (stopLocations.contains(stop.stopCode)) {
      return false;
    } else {
      stopLocations.add(stop.stopCode!);
    }

    final bounds = animatedMapController.mapController.camera.visibleBounds;
    return bounds.contains(LatLng(stop.stopLat!, stop.stopLon!));
  }

  double calculateRadius() {
    switch (animatedMapController.mapController.camera.zoom) {
      case < 16:
        return 8;
      case < 16.5:
        return 12;
      case < 17:
        return 14;
      default:
        return 18;
    }
  }
}