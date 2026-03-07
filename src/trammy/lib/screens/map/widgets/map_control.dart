import 'package:flutter/material.dart';
import 'package:flutter_map/flutter_map.dart';
import 'package:flutter_map_animations/flutter_map_animations.dart';
import 'package:latlong2/latlong.dart';
import 'package:trammy/models/gtfs/stop.dart';
import 'package:trammy/screens/map/widgets/pulsing_user_marker.dart';
import 'package:trammy/screens/map/widgets/stops_layer.dart';
import 'package:url_launcher/url_launcher.dart';

class MapControl extends StatelessWidget {
  final AnimatedMapController animatedMapController;
  final LatLng initialCenter;
  final double initialZoom;
  final List<GTFSStopRouteInfo> stops;
  final LatLng? userLocation;
  final GTFSStopRouteInfo? selectedStop;

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
    this.selectedStop,
  });

  Widget renderMapTheme(BuildContext context) {
    final tileLayer =  TileLayer(
              urlTemplate:
                  'https://cartodb-basemaps-{s}.global.ssl.fastly.net/rastertiles/voyager/{z}/{x}/{y}@3x.png',
              subdomains: ['a', 'b', 'c'],
              userAgentPackageName: 'Trammy/5.0 (trammy@outlook.com)',
            );

    if (Theme.of(context).brightness == Brightness.dark) { 
     return ColorFiltered(
          colorFilter: const ColorFilter.matrix(<double>[
            -0.2126, -0.5152, -0.0722, 0, 255, // Red channel
            -0.2126, -0.5152, -0.0722, 0, 255, // Green channel
            -0.2126, -0.5152, -0.0722, 0, 255, // Blue channel
            0,       0,       0,       1, 0,   // Alpha channel
          ]),
        
          child: tileLayer
      );
    }

    return tileLayer;
  }

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
          renderMapTheme(context),
          if (stops.isNotEmpty) StopsLayer(
            animatedMapController: animatedMapController,
            stops: stops,
            onStopTapped: onStopTapped,
            selectedStop: selectedStop
          ),
          if (userLocation != null)
            MarkerLayer(
              markers: [
                Marker(
                  point: userLocation!,
                  width: 80,
                  height: 80,
                  child: PulsingUserMarker()
                ),
              ],
            ),
              Positioned(
      bottom: 0,
      right: 0,
      child: GestureDetector(
        onTap: () async {
          final uri = Uri.parse(
            'https://www.openstreetmap.org/copyright',
          );
          await launchUrl(uri);
        },
        child: Container(
          padding: const EdgeInsets.symmetric(horizontal: 6, vertical: 3),
          color: Colors.white70,
          child: const Text(
            '© OpenStreetMap',
            style: TextStyle(fontSize: 12),
          ),
        ),
      ),
              )
                
        ],
      );
  }
}