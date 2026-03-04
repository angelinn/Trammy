import 'dart:collection';

import 'package:flutter/material.dart';
import 'package:flutter_map/flutter_map.dart';
import 'package:flutter_map_animations/flutter_map_animations.dart';
import 'package:geolocator/geolocator.dart';
import 'package:latlong2/latlong.dart';
import 'package:shared_preferences/shared_preferences.dart';
import 'package:trammy/controllers/map_screen_controller.dart';
import 'package:trammy/models/gtfs/route.dart';
import 'package:trammy/models/gtfs/stop.dart';
import 'package:trammy/screens/map/widgets/map_control.dart';
import 'package:trammy/screens/map/widgets/stop_search_bar.dart';
import 'package:trammy/services/common.dart';
import 'package:trammy/services/gtfs_service.dart';

class MapScreen extends StatefulWidget {
  const MapScreen({super.key, required this.title});

  final String title;

  @override
  State<MapScreen> createState() => MapScreenState();
}

class MapScreenState extends State<MapScreen> with TickerProviderStateMixin {
  late TextEditingController searchController;
  bool stopsLoaded = false;
  MapCamera? currentPosition;
  LatLng? userLocation;
  late AnimatedMapController animatedMapController;
  final MapScreenController mapScreenController = MapScreenController();

  Set<String> stopLocations = {};

  @override
  void initState() {
    super.initState();
    animatedMapController = AnimatedMapController(
      vsync: this,
      duration: const Duration(milliseconds: 500),
      curve: Curves.easeInOut,
    );

    loadLastPosition();

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

  Future<LatLng?> getUserLocation() async {
    bool serviceEnabled;
    LocationPermission permission;

    // Check if location services are enabled
    serviceEnabled = await Geolocator.isLocationServiceEnabled();
    if (!serviceEnabled) return null;

    // Check permission
    permission = await Geolocator.checkPermission();
    if (permission == LocationPermission.denied) {
      permission = await Geolocator.requestPermission();
      if (permission == LocationPermission.denied) return null;
    }
    if (permission == LocationPermission.deniedForever) return null;

    // Get current position
    final pos = await Geolocator.getCurrentPosition(
      desiredAccuracy: LocationAccuracy.high,
    );
    return LatLng(pos.latitude, pos.longitude);
  }

  void _goToCurrentLocation() async {
    var location = await getUserLocation();
    if (location != null) {
      setState(() {
        userLocation = location;
      });

      animatedMapController.animateTo(dest: location, zoom: 18); // Zoom to user
    }
  }

  Future<void> onStopTapped(GTFSStopRouteInfo stop) async {
    animatedMapController.animateTo(
      dest: LatLng(stop.stopLat!, stop.stopLon!),
      zoom: 18,
    );

    showArrivals(stop);
  }

  void showArrivals(GTFSStopRouteInfo stop) {
    showModalBottomSheet(
      context: context,
      isScrollControlled: true,
      builder: (_) {
        Map<StopInfoKey, List<DateTime>>? updates;
        bool isLoading = true;

        final now = DateTime.now();

        return StatefulBuilder(
          builder: (context, setModalState) {
            // Fetch data only once
            if (isLoading) {
              mapScreenController.getUpdatesForStop(stop).then((result) {
                setModalState(() {
                  updates = result;
                  isLoading = false;
                });
              });
            }

            return DraggableScrollableSheet(
              initialChildSize: 0.5,
              minChildSize: 0.35,
              maxChildSize: 0.75,
              expand: false,
              builder: (context, scrollController) {
                return Container(
                  decoration: const BoxDecoration(color: Colors.white, borderRadius: BorderRadius.vertical(top: Radius.circular(16))),
                  child: Padding(
                    padding: const EdgeInsets.all(16),
                    child: isLoading
                        ? Column(
                            mainAxisSize: MainAxisSize.min,
                            children: [
                              Row(
                                children: [
                                  Container(
                                    width: 36,
                                    height: 36,
                                    decoration: BoxDecoration(color: colorFromHex(stop.getDominantColor()!), shape: BoxShape.circle),
                                    child: Icon(GTFSService.getStopIcon(stop.getDominantType()!),color: Colors.white, size: 20),
                                  ),
                                  const SizedBox(width: 8),
                                  Expanded(
                                    child: Text("${stop.stopName} (${stop.stopCode})", style: const TextStyle(fontSize: 18, fontWeight: FontWeight.bold))
                                  ),
                                ],
                              ),
                              const SizedBox(height: 24),
                              const Center(child: CircularProgressIndicator()),
                            ],
                          )
                        : _buildArrivalsContent(stop, updates ?? {}, now, scrollController)
                  ),
                );
              },
            );
          },
        );
      },
    );
  }

  Widget _buildArrivalsContent(
    GTFSStopRouteInfo stop,
    Map<StopInfoKey, List<DateTime>> updates,
    DateTime now,
    ScrollController scrollController,
  ) {
    return SingleChildScrollView(
      controller: scrollController,
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              Container(
                width: 36,
                height: 36,
                decoration: BoxDecoration(color: colorFromHex(stop.getDominantColor()!), shape: BoxShape.circle),
                child: Icon(GTFSService.getStopIcon(stop.getDominantType()!), color: Colors.white, size: 20)
              ),
              const SizedBox(width: 8),
              Text("${stop.stopName} (${stop.stopCode})", style: const TextStyle(fontSize: 18, fontWeight: FontWeight.bold)),
            ],
          ),
          const SizedBox(height: 12),

          if (updates.isEmpty)
            const Text("Няма пристигащи скоро превозни средства.")
          else
            ...buildArrivalsList(updates, now),
        ],
      ),
    );
  }

  List<Widget> buildArrivalsList(
    Map<StopInfoKey, List<DateTime>> updates,
    DateTime now,
  ) {
    final sortedUpdates = updates.entries.toList()
      ..sort((a, b) {
        final vehicleCompare = a.key.route.routeType!.compareTo(
          b.key.route.routeType!,
        );

        if (vehicleCompare != 0) return vehicleCompare;

        int parseLine(String line) => int.tryParse(line) ?? 1337;

        return parseLine(
          a.key.route.routeShortName!,
        ).compareTo(parseLine(b.key.route.routeShortName!));
      });

    return sortedUpdates.map((entry) {
      if (entry.value.isEmpty) {
        return const SizedBox.shrink();
      }

      final minutes = entry.value
          .map((date) {
            var diff = date.difference(now).inMinutes;
            if (diff < 0) diff = 0;
            return "$diff мин";
          })
          .take(3)
          .toList();

      return buildArrivalCard(entry, minutes);
    }).toList();
  }

  Card buildArrivalCard(
    MapEntry<StopInfoKey, List<DateTime>> entry,
    List<String> minutes,
  ) {
    return Card(
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
      elevation: 3,
      margin: const EdgeInsets.symmetric(vertical: 6),
      child: Padding(
        padding: const EdgeInsets.symmetric(vertical: 8, horizontal: 12),
        child: Row(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Row(
                    children: [
                      // Route name with colored background
                      Container(
                        padding: const EdgeInsets.symmetric(
                          vertical: 4,
                          horizontal: 8,
                        ),
                        decoration: BoxDecoration(
                          color: colorFromHex(entry.key.route.routeColor!),
                          borderRadius: BorderRadius.circular(6),
                        ),
                        child: Text(
                          entry.key.route.routeShortName!,
                          style: const TextStyle(
                            color: Colors.white,
                            fontWeight: FontWeight.bold,
                          ),
                        ),
                      ),
                      const SizedBox(height: 10),
                      Padding(
                        padding: const EdgeInsets.only(left: 8.0),
                        child: Text(
                          entry.key.direction,
                          style: const TextStyle(fontSize: 14),
                        ),
                      ),
                    ],
                  ),
                  const SizedBox(height: 6),
                  // Upcoming times as chips
                  Wrap(
                    spacing: 6,
                    children: minutes
                        .map(
                          (time) => Chip(
                            label: Text(time),
                            visualDensity: VisualDensity.compact,
                          ),
                        )
                        .toList(),
                  ),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }

  void onMapTapped(TapPosition tapPosition, LatLng latlng) {
    const double maxDistanceMeters = 20;

    GTFSStopRouteInfo? closest;
    double minDistance = double.infinity;

    for (final stop in GTFSService.stopsByCode) {
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

  @override
  Widget build(BuildContext context) {
    if (!positionLoaded) {
      return const Scaffold(body: Center(child: CircularProgressIndicator()));
    }
    stopLocations = {};
    return Scaffold(
      body: Stack(
        children: [ 
          MapControl(
            animatedMapController: animatedMapController, 
            initialCenter: const LatLng(42.6977, 23.3219), // Sofia
            initialZoom: 16,
            onMapTapped: onMapTapped,
            onStopTapped: onStopTapped,
            onMoveEnd: onMoveEnd,
            stops: GTFSService.stopsByCode,
        ),
          // Bottom search bar
          Positioned(left: 32, right: 32, bottom: 32, child: StopSearchBar(stops: GTFSService.stopsByCode, onStopSearch: onStopSearch)),
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

  void onMoveEnd() {
    setState(() {
      currentPosition = animatedMapController.mapController.camera;
    });
  }

  Future<void> onPositionChanged(MapCamera camera, bool hasGesture) async {
    var prefs = await SharedPreferences.getInstance();
    prefs.setDouble('lastLat', camera.center.latitude);
    prefs.setDouble('lastLng', camera.center.longitude);
    prefs.setDouble('lastZoom', camera.zoom);
  }

  bool positionLoaded = false;
  Future<void> loadLastPosition() async {
    var prefs = await SharedPreferences.getInstance();
    lastLat = prefs.getDouble('lastLat');
    lastLng = prefs.getDouble('lastLng');
    lastZoom = prefs.getDouble('lastZoom');

    setState(() {
      positionLoaded = true;
    });
  }

  double? lastLat;
  double? lastLng;
  double? lastZoom;

  void onStopSearch(String value) {
    final query = value.toLowerCase();
    final matches = GTFSService.stopsByCode.where(
      (s) =>
          s.stopName != null &&
          s.stopCode != null &&
          "${s.stopName!.toLowerCase()} (${s.stopCode!.toLowerCase()})" ==
              query,
    );

    if (matches.isEmpty) {
      ScaffoldMessenger.of(
        context,
      ).showSnackBar(const SnackBar(content: Text('Няма намерени спирки')));
    } else {
      final stop = matches.first;
      onStopTapped(stop);
      setState(() {
        currentPosition = animatedMapController.mapController.camera;
      });
    }
  }
}
