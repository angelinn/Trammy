
import 'package:flutter/material.dart';
import 'package:flutter_map/flutter_map.dart';
import 'package:flutter_map_animations/flutter_map_animations.dart';
import 'package:latlong2/latlong.dart';
import 'package:shared_preferences/shared_preferences.dart';
import 'package:trammy/controllers/map_screen_controller.dart';
import 'package:trammy/models/gtfs/stop.dart';
import 'package:trammy/screens/map/widgets/map_control.dart';
import 'package:trammy/screens/map/widgets/stop_search_bar.dart';
import 'package:trammy/screens/map/widgets/stop_sheet.dart';
import 'package:trammy/services/gtfs_service.dart';
import 'package:trammy/services/location_service.dart';

class MapScreen extends StatefulWidget {
  const MapScreen({super.key, required this.title});

  final String title;

  @override
  State<MapScreen> createState() => MapScreenState();
}

class MapScreenState extends State<MapScreen> with TickerProviderStateMixin {
  bool stopsLoaded = false;
  MapCamera? currentPosition;
  LatLng? userLocation;

  late AnimatedMapController animatedMapController;
  final MapScreenController mapScreenController = MapScreenController();

  double? lastLat;
  double? lastLng;
  double? lastZoom;

  bool positionLoaded = false;
  Set<String> stopLocations = {};

  @override
  void initState() {
    super.initState();
    initialize();
  }

  Future<void> initialize() async {
    animatedMapController = AnimatedMapController(
      vsync: this,
      duration: const Duration(milliseconds: 500),
      curve: Curves.easeInOut,
    );

    loadLastPosition();

    print('[MapScreen] initState()');
    await mapScreenController.initialize();
    setState(() {
      stopsLoaded = true;
    });
  }

  void goToCurrentLocation() async {
    await LocationService.startTracking((position) {
      setState(() {
        userLocation = position;
      });

      animatedMapController.animateTo(dest: position, zoom: 18);
    });
  }

  Future<void> onStopTapped(GTFSStopRouteInfo stop) async {
    animatedMapController.animateTo(
      dest: LatLng(stop.stopLat!, stop.stopLon!),
      zoom: 18,
    );
    showModalBottomSheet(
      context: context,
      isScrollControlled: true,
      barrierColor: Colors.black.withOpacity(0.1),
      builder: (_) => StopSheet(stop: stop, mapScreenController: mapScreenController)
    );
  }

  void onMapTapped(TapPosition tapPosition, LatLng latlng) {
    GTFSStopRouteInfo? stop = GTFSService.findNearestStop(latlng);
    if (stop != null) {
      onStopTapped(stop);
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
            userLocation: userLocation
        ),
          // Bottom search bar
          Positioned(left: 32, right: 32, bottom: 35, child: StopSearchBar(stops: GTFSService.stopsByCode, onStopSearch: onStopSearch)),
          Positioned(
            bottom: 106, // slightly above search bar
            right: 16,
            child: FloatingActionButton(
              onPressed: goToCurrentLocation,
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

  Future<void> loadLastPosition() async {
    var prefs = await SharedPreferences.getInstance();
    lastLat = prefs.getDouble('lastLat');
    lastLng = prefs.getDouble('lastLng');
    lastZoom = prefs.getDouble('lastZoom');

    setState(() {
      positionLoaded = true;
    });
  }

  void onStopSearch(String value) {
    final matches = GTFSService.searchStop(value.toLowerCase());

    if (matches.isEmpty) {
      ScaffoldMessenger.of(context,).showSnackBar(const SnackBar(content: Text('Няма намерени спирки')));
    } else {
      onStopTapped(matches.first);

      setState(() {
        currentPosition = animatedMapController.mapController.camera;
      });
    }
  }
}
