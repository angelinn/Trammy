import 'package:flutter/material.dart';
import 'package:flutter_map/flutter_map.dart';
import 'package:flutter_map_animations/flutter_map_animations.dart';
import 'package:latlong2/latlong.dart';
import 'package:trammy/models/gtfs/stop.dart';
import 'package:trammy/screens/map/widgets/stops_layer.dart';

class MapControl extends StatelessWidget {
  final AnimatedMapController animatedMapController;
  final LatLng initialCenter;
  final double initialZoom;
  final List<GTFSStopRouteInfo> stops;
  final LatLng? userLocation;

  final void Function(GTFSStopRouteInfo stop) onStopTapped;
  final void Function(MapCamera camera, bool hasGesture)? onPositionChanged;
  final VoidCallback? onMoveEnd;
  final void Function(TapPosition, LatLng)? onMapTapped;

    const MapControl({
    super.key,
    required this.animatedMapController,
    required this.initialCenter,
    required this.initialZoom,
    required this.stops,
    required this.onStopTapped,
    this.userLocation,
    this.onPositionChanged,
    this.onMoveEnd,
    this.onMapTapped,
  });

  @override
  Widget build(BuildContext context) {
     return FlutterMap(
      mapController: animatedMapController.mapController,
      options: MapOptions(
        initialCenter: initialCenter,
        initialZoom: initialZoom,
        maxZoom: 19,
        interactionOptions: const InteractionOptions(
          flags: InteractiveFlag.all & ~InteractiveFlag.rotate,
        ),
        onPositionChanged: onPositionChanged,
        onTap: onMapTapped,
        onMapEvent: (event) => {
          if (event is MapEventMoveEnd) {
              debugPrint('Map move ended ${animatedMapController.mapController.camera.zoom}'),
              onMoveEnd?.call(),
            },
        },
      ),
      children: [
          TileLayer(
            urlTemplate:
                'https://cartodb-basemaps-{s}.global.ssl.fastly.net/rastertiles/voyager/{z}/{x}/{y}@3x.png',
            subdomains: ['a', 'b', 'c'],
            userAgentPackageName: 'Trammy/5.0 (trammy@outlook.com)',
          ),
          if (stops.isNotEmpty) StopsLayer(animatedMapController: animatedMapController, stops: stops, onStopTapped: onStopTapped),
          if (userLocation != null)
            MarkerLayer(
              markers: [
                Marker(
                  point: userLocation!,
                  width: 40,
                  height: 40,
                  child: const Icon(
                    Icons.my_location,
                    color: Colors.blue,
                    size: 32,
                  ),
                ),
              ],
            ),
        ],
      );
  }
}