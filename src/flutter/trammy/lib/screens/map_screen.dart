import 'package:flutter/material.dart';
import 'package:flutter_map/flutter_map.dart';
import 'package:latlong2/latlong.dart';
import 'package:trammy/db/gtfs_repository.dart';
import 'package:trammy/models/gtfs/route.dart';
import 'package:trammy/services/gtfs_service.dart';

class MapScreen extends StatefulWidget {
  const MapScreen({super.key, required this.title});

  final String title;

  @override
  State<MapScreen> createState() => MapScreenState();
}

class MapScreenState extends State<MapScreen> {
  final TextEditingController searchController = TextEditingController();
  GTFSRepository? repo;
  List<Stop> stops = [];
  bool mapReady = true;
  MapCamera? currentPosition;
  final MapController mapController = MapController();

  bool _isStopInView(Stop stop) {
    if (mapController.camera.zoom < 15.5) return false;

    final bounds = mapController.camera.visibleBounds;
    return bounds.contains(LatLng(stop.stopLat!, stop.stopLon!));
  }

  @override
  void initState() {
    super.initState();

    GTFSService.init().then((db) {
      repo = GTFSRepository(db: db!);
      repo?.getStops().then((stops) {
        debugPrint('[MapScreen] Loaded ${stops.length} stops from database');
        setState(() {
          this.stops = stops;
        });
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

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: Stack(
        children: [
          FlutterMap(
            mapController: mapController,
            options: MapOptions(
              initialCenter: const LatLng(42.6977, 23.3219), // Sofia
              initialZoom: 16,
              onPositionChanged: (position, hasGesture) {

              },
              onMapReady: () => {

              },
              onMapEvent: (event) => {
                if (event is MapEventMoveEnd) {
                  debugPrint('Map move ended ${mapController.camera.zoom}'),
                
                  // This will trigger a rebuild and update the visible stops
                  setState(() {
                    currentPosition = mapController.camera;
                  }),
                }
              },
            ),
            children: [
              TileLayer(
                urlTemplate:
                    'https://cartodb-basemaps-{s}.global.ssl.fastly.net/rastertiles/voyager/{z}/{x}/{y}@3x.png',
                subdomains: ['a', 'b', 'c'],
                userAgentPackageName: 'Trammy/5.0 (trammy@outlook.com)',
              ),
              if (stops.isNotEmpty && mapReady)
                CircleLayer(
                  circles: stops.where((s) => 
                  s.stopLat != null && s.stopLon != null && _isStopInView(s)).map((stop) {
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
                      color: Colors.deepPurple.withOpacity(0.7),
                      borderStrokeWidth: 1.0,
                      borderColor: Colors.deepPurple,
                      radius: radius, // pixels
                    );
                  }).toList(),
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
