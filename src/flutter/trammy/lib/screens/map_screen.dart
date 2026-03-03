import 'package:flutter/material.dart';
import 'package:flutter_map/flutter_map.dart';
import 'package:latlong2/latlong.dart';
import 'package:trammy/controllers/map_screen_controller.dart';
import 'package:trammy/models/gtfs/route.dart';
import 'package:trammy/models/gtfs/stop.dart';
import 'package:trammy/services/gtfs_service.dart';

class MapScreen extends StatefulWidget {
  const MapScreen({super.key, required this.title});

  final String title;

  @override
  State<MapScreen> createState() => MapScreenState();
}

class MapScreenState extends State<MapScreen> {
  final TextEditingController searchController = TextEditingController();
  bool stopsLoaded = false;
  MapCamera? currentPosition;
  final MapController mapController = MapController();
  final MapScreenController mapScreenController = MapScreenController();

    Set<String> stopLocations = {};
  bool _isStopInView(GTFSStopRouteInfo stop) {
    if (mapController.camera.zoom < 14) return false;

    if (stopLocations.contains(stop.stopCode)) {
      return false;
    } else {
      stopLocations.add(stop.stopCode!);
    }

    final bounds = mapController.camera.visibleBounds;
    return bounds.contains(LatLng(stop.stopLat!, stop.stopLon!));
  }

  @override
  void initState() {
    super.initState();

    print('[MapScreen] initState()');
    mapScreenController.initialize().then((_) {
      setState(() {
        stopsLoaded = true;
      });
    });
  }

  @override
  void dispose() {
    searchController.dispose();
    super.dispose();
  }

  void _goToCurrentLocation() {
    // TODO: implement logic to move the map to user's current location
    print('FAB pressed: Go to current location');
  }

  Future<void> onStopTapped(GTFSStopRouteInfo stop) async {
    var updates = await mapScreenController.getUpdatesForStop(stop);
    showArrivals(stop, updates);
  }

  void showArrivals(
    GTFSStopRouteInfo stop,
    Map<GTFSRoute, List<DateTime>> updates,
  ) {
    print('Showing arrivals for stop ${stop.stopName}');
    String _formatTime(DateTime dt) {
      final hour = dt.hour.toString().padLeft(2, '0');
      final minute = dt.minute.toString().padLeft(2, '0');
      return "$hour:$minute";
    }

    final now = DateTime.now();

    showModalBottomSheet(
      context: context,
      builder: (_) {
        return Padding(
          padding: const EdgeInsets.all(16),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(
                "${stop.stopName} (${stop.stopCode})",
                style: const TextStyle(
                  fontSize: 18,
                  fontWeight: FontWeight.bold,
                ),
              ),
              const SizedBox(height: 12),

              if (updates.isEmpty)
                const Text("Няма пристигащи скоро превозни средства.")
              else
                ...updates.entries.take(5).map((entry) {
                  if (entry.value.isEmpty) return const SizedBox.shrink();

                  final dt = entry.value.first;
                  final minutes = entry.value
                      .skip(1)
                      .map((dt) => "${dt.difference(now).inMinutes} минути")
                      .take(3)
                      .join(", ");

                  return ListTile(
                    leading: const Icon(Icons.tram),
                    title: Text(
                      "${entry.key.routeShortName} ${_formatTime(dt)}",
                    ),
                    subtitle: Text(minutes),
                  );
                }),
            ],
          ),
        );
      },
    );
  }

  void onMapTapped(TapPosition tapPosition, LatLng latlng) {
    const double maxDistanceMeters = 20;

    GTFSStopRouteInfo? closest;
    double minDistance = double.infinity;

    for (final stop in GTFSService.stops) {
      if (stop.stopLat == null || stop.stopLon == null) continue;
      final distance = const Distance().as(
        LengthUnit.Meter,
        latlng,
        LatLng(stop.stopLat!, stop.stopLon!),
      );

      if (distance < minDistance && distance < maxDistanceMeters) {
        minDistance = distance;
        closest = stop;
      }
    }

    if (closest != null) {
      print(
        'Tapped near stop: ${closest.stopName} (${closest.stopId}), distance: ${minDistance.toStringAsFixed(1)}m',
      );
      onStopTapped(closest);
    } else {
      print(
        'Tapped at ${latlng.latitude.toStringAsFixed(5)}, ${latlng.longitude.toStringAsFixed(5)}, no nearby stop (closest is ${minDistance.toStringAsFixed(1)}m away)',
      );
    }
  }
Color colorFromHex(String hexString) {
  final buffer = StringBuffer();
  if (hexString.length == 6 || hexString.length == 7) buffer.write('ff'); // add opacity if missing
  buffer.write(hexString.replaceFirst('#', ''));
  return Color(int.parse(buffer.toString(), radix: 16));
}
  @override
  Widget build(BuildContext context) {
    stopLocations = {};
    return Scaffold(
      body: Stack(
        children: [
          FlutterMap(
            mapController: mapController,
            options: MapOptions(
              initialCenter: const LatLng(42.6977, 23.3219), // Sofia
              initialZoom: 16,
              maxZoom: 19,
              interactionOptions: const InteractionOptions(
                flags: InteractiveFlag.all & ~InteractiveFlag.rotate,
              ),
              onPositionChanged: (position, hasGesture) {},
              onTap: onMapTapped,
              onMapEvent: (event) => {
                if (event is MapEventMoveEnd)
                  {
                    debugPrint('Map move ended ${mapController.camera.zoom}'),

                    // This will trigger a rebuild and update the visible stops
                    setState(() {
                      currentPosition = mapController.camera;
                    }),
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
              if (GTFSService.stops.isNotEmpty)
                CircleLayer(
                  circles: GTFSService.stops
                      .where(
                        (s) =>
                            s.stopLat != null &&
                            s.stopLon != null &&
                            _isStopInView(s),
                      )
                      .map((stop) {
                        final zoom = mapController.camera.zoom; // current zoom
                        final double radius;
                        switch (zoom) {
                          case < 16:
                            radius = 4;
                            break;
                          case < 16.5:
                            radius = 6;
                            break;
                          case < 17:
                            radius = 7;
                            break;
                          default:
                            radius = 9;
                            break;
                        }
                        return CircleMarker(
                          point: LatLng(stop.stopLat!, stop.stopLon!),
                          color: colorFromHex(stop.routeColor!).withOpacity(0.7),
                          borderStrokeWidth: 1.0,
                          borderColor: colorFromHex(stop.routeColor!),
                          radius: radius, // pixels
                        );
                      })
                      .toList(),
                ),
            ],
          ),
          // Bottom search bar
          Positioned(
            left: 32,
            right: 32,
            bottom: 32,
            child: Material(
              elevation: 4,
              borderRadius: BorderRadius.circular(8),
              child: TextField(
                controller: searchController,
                decoration: InputDecoration(
                  hintText: 'Търсене на спирка...',
                  prefixIcon: const Icon(Icons.search),
                  border: InputBorder.none,
                  contentPadding: const EdgeInsets.symmetric(
                    vertical: 14,
                    horizontal: 16,
                  ),
                ),
                onSubmitted: (query) {
                  // TODO: Implement search logic
                  debugPrint('Search for: $query');
                },
              ),
            ),
          ),
          Positioned(
            bottom: 106, // slightly above search bar
            right: 16,
            child: FloatingActionButton(
              onPressed: _goToCurrentLocation,
              child: const Icon(Icons.my_location),
            ),
          ),
        ],
      ),
    );
  }
}
