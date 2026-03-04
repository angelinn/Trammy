import 'package:flutter/material.dart';
import 'package:flutter_map/flutter_map.dart';
import 'package:geolocator/geolocator.dart';
import 'package:latlong2/latlong.dart';
import 'package:shared_preferences/shared_preferences.dart';
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
  late TextEditingController searchController;
  bool stopsLoaded = false;
  MapCamera? currentPosition;
  LatLng? userLocation;
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

      mapController.move(location, 18); // Zoom to user
    }
  }

  Future<void> onStopTapped(GTFSStopRouteInfo stop) async {
    mapController.move(LatLng(stop.stopLat!, stop.stopLon!), 18);

    var updates = await mapScreenController.getUpdatesForStop(stop);
    showArrivals(stop, updates);
  }

  void showArrivals(
    GTFSStopRouteInfo stop,
    Map<StopInfoKey, List<DateTime>> updates,
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
      isScrollControlled: true,
      builder: (_) {
        return DraggableScrollableSheet(
          initialChildSize: updates.entries.length > 3
              ? (updates.entries.length > 5 ? 0.75 : 0.5)
              : 0.35,
          minChildSize: 0.35,
          maxChildSize: 0.75,
          expand: false,
          builder: (context, scrollController) {
            return LayoutBuilder(
              builder: (context, constraints) {
                // Wrap content in SingleChildScrollView
                return Container(
                  decoration: const BoxDecoration(
                    color: Colors.white,
                    borderRadius: BorderRadius.vertical(
                      top: Radius.circular(16),
                    ),
                  ),
                  child: SingleChildScrollView(
                    controller: scrollController, // important!
                    child: Padding(
                      padding: const EdgeInsets.all(16),
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Row(
                          children: [
                            Container(
                        width: 36,
                        height: 36,
                        decoration: BoxDecoration(
                          color: colorFromHex(stop.getDominantColor()!), // route-specific color
                          shape: BoxShape.circle,
                        ),
                        child: Icon(
                          GTFSService.getStopIcon(stop.getDominantType()!),
                          color: Colors.white, // icon stands out on colored background
                          size: 20,
                        ),
                      ),
                            const SizedBox(width: 8),
                            Text(
                              "${stop.stopName} (${stop.stopCode})",
                              style: const TextStyle(
                                fontSize: 18,
                                fontWeight: FontWeight.bold,
                              ),
                            ),
                          ],
                        ),
                          const SizedBox(height: 12),
                          if (updates.isEmpty)
                            const Text(
                              "Няма пристигащи скоро превозни средства.",
                            )
                          else
                            ...(updates.entries.toList()..sort((a, b) { 
                             final vehicleCompare = a.key.route.routeType!.compareTo(b.key.route.routeType!);
                                  if (vehicleCompare != 0) return vehicleCompare;

                                  // Then sort by LineName as integer, fallback to 1337
                                  int parseLine(String line) {
                                    final num = int.tryParse(line);
                                    return num ?? 1337;
                                  }

                                  return parseLine(a.key.route.routeShortName!).compareTo(parseLine(b.key.route.routeShortName!));
                            })).map((entry) {
                              if (entry.value.isEmpty)
                                return const SizedBox.shrink();

                              final dt = entry.value.first;
                              final minutes = entry.value
                                  .map(
                                    (date) {
                                      var difference = date.difference(now).inMinutes;
                                      if (difference < 0) difference = 0;

                                       return "$difference мин";
                                    }
                                  )
                                  .take(3);

                              return Card(
                                shape: RoundedRectangleBorder(
                                  borderRadius: BorderRadius.circular(12),
                                ),
                                elevation: 3,
                                margin: const EdgeInsets.symmetric(vertical: 6),
                                child: Padding(
                                  padding: const EdgeInsets.symmetric(
                                    vertical: 8,
                                    horizontal: 12,
                                  ),
                                  child: Row(
                                    crossAxisAlignment: CrossAxisAlignment.start,
                                    children: [
                                      Expanded(
                                        child: Column(
                                          crossAxisAlignment: CrossAxisAlignment.start,
                                          children: [Row(
                                            children:[
                                            // Route name with colored background
                                            Container(
                                              padding:
                                                  const EdgeInsets.symmetric(vertical: 4,horizontal: 8),
                                              decoration: BoxDecoration(color: colorFromHex(entry.key.route.routeColor!), borderRadius: BorderRadius.circular(6),),
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
                                              style: const TextStyle(
                                                fontSize: 14
                                              ),
                                            ),
                                            )
                                          ]),
                                            const SizedBox(height: 6),
                                            // Upcoming times as chips
                                            Wrap(
                                              spacing: 6,
                                              children: minutes
                                                  .map(
                                                    (time) => Chip(
                                                      label: Text(time),
                                                      visualDensity:
                                                          VisualDensity.compact,
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
                            }),
                        ],
                      ),
                    ),
                  ),
                );
              },
            );
          },
        );
      },
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

  Color colorFromHex(String hexString) {
    final buffer = StringBuffer();
    if (hexString.length == 6 || hexString.length == 7)
      buffer.write('ff'); // add opacity if missing
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
              onPositionChanged: onPositionChanged,
              onMapReady: onMapReady,
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
              if (GTFSService.stopsByCode.isNotEmpty)
                MarkerLayer(
                  markers: GTFSService.stopsByCode
                      .where(
                        (s) =>
                            s.stopLat != null &&
                            s.stopLon != null &&
                            _isStopInView(s),
                      )
                      .map((stop) {
                        final zoom = mapController.camera.zoom; // current zoom
                        double radius;
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

                        radius *= 2;
                        return Marker(
                          point: LatLng(stop.stopLat!, stop.stopLon!),
                          width: radius,
                          height: radius,

                          child: Material(
                            color: Colors.transparent,
                            shape: const CircleBorder(),
                            child: InkWell(
                              customBorder: const CircleBorder(),
                              onTap: () => onStopTapped(stop),
                              child: Container(
                                width: radius,
                                height: radius,
                                decoration: BoxDecoration(
                                  shape: BoxShape.circle,
                                  color: colorFromHex(
                                    stop.getDominantColor()!,
                                  ).withOpacity(0.8),
                                  border: Border.all(
                                    color: colorFromHex(
                                      stop.getDominantType()!.toString(),
                                    ),
                                    width: 1,
                                  ),
                                ),
                              ),
                            ),
                          ),
                        );
                      })
                      .toList(),
                ),
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
          ),
          // Bottom search bar
          Positioned(left: 32, right: 32, bottom: 32, child: buildStopSearch()),
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

  Future<void> onPositionChanged(MapCamera camera, bool hasGesture) async {
    var prefs = await SharedPreferences.getInstance();
    prefs.setDouble('lastLat', camera.center.latitude);
    prefs.setDouble('lastLng', camera.center.longitude);
    prefs.setDouble('lastZoom', camera.zoom);
  }

  Future<void> loadLastPosition() async {
    var prefs = await SharedPreferences.getInstance();
    lastLat = prefs.getDouble('lastLat');
    lastLng = prefs.getDouble('lastLng');
    lastZoom = prefs.getDouble('lastZoom');
  }

  double? lastLat;
  double? lastLng;
  double? lastZoom;

  void onMapReady() {
    if (lastLat != null && lastLng != null && lastZoom != null) {
      mapController.move(LatLng(lastLat!, lastLng!), lastZoom!);
    }
  }

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
        currentPosition = mapController.camera;
      });
    }

    searchController.clear();
    searchFocusNode.unfocus();
  }

  late FocusNode searchFocusNode;
  Widget buildStopSearch() {
    return Autocomplete<String>(
      optionsViewOpenDirection: OptionsViewOpenDirection.up,
      optionsBuilder: (TextEditingValue textEditingValue) {
        if (textEditingValue.text.isEmpty) {
          return const Iterable<String>.empty();
        }
        return GTFSService.stopsByCode
            .where(
              (stop) =>
                  stop.stopName!.toLowerCase().contains(
                    textEditingValue.text.toLowerCase(),
                  ) ||
                  stop.stopCode!.toLowerCase().contains(
                    textEditingValue.text.toLowerCase(),
                  ),
            )
            .map((s) => "${s.stopName!} (${s.stopCode})");
      },
      onSelected: onStopSearch,
      fieldViewBuilder:
          (context, textController, focusNode, onEditingComplete) {
            searchController = textController;
            searchFocusNode = focusNode;
            return Material(
              elevation: 4,
              borderRadius: BorderRadius.circular(16),
              color: Colors.white, // make the box opaque
              child: TextField(
                controller: searchController,
                focusNode: focusNode,
                onEditingComplete: onEditingComplete,
                decoration: const InputDecoration(
                  hintText: 'Търсене на спирка...',
                  prefixIcon: Icon(Icons.search),
                  border: InputBorder.none,
                  contentPadding: EdgeInsets.symmetric(
                    vertical: 14,
                    horizontal: 16,
                  ),
                ),
              ),
            );
          },
    );
  }
}
