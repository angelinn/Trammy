import 'package:flutter/material.dart';
import 'package:flutter_map/flutter_map.dart';
import 'package:latlong2/latlong.dart';
import 'package:trammy/screens/map/widgets/vehicle_market.dart';
import 'package:trammy/services/common.dart';
import 'package:trammy/services/gtfs_service.dart';

class AnimatedVehiclesLayer extends StatefulWidget {
  final Set<String> vehiclePositions;

  const AnimatedVehiclesLayer({
    super.key,
    required this.vehiclePositions,
  });

  @override
  State<AnimatedVehiclesLayer> createState() =>
      _AnimatedVehiclesLayerState();
}

class _AnimatedVehiclesLayerState
    extends State<AnimatedVehiclesLayer>
    with SingleTickerProviderStateMixin {
  late final AnimationController _controller;

  final Map<String, LatLng> _fromPositions = {};
  final Map<String, LatLng> _toPositions = {};

  DateTime? _lastUpdate;

  @override
  void initState() {
    super.initState();

    _controller = AnimationController(
      vsync: this,
      duration: const Duration(seconds: 10),
    )..addListener(() {
        if (mounted) {
          setState(() {});
        }
      });

    GTFSService.vehiclesNotifier.addListener(_onVehiclesUpdated);

    // Handle vehicles that already exist when this widget is created.
    _onVehiclesUpdated();
  }

  @override
  void dispose() {
    GTFSService.vehiclesNotifier.removeListener(_onVehiclesUpdated);
    _controller.dispose();
    super.dispose();
  }

  void _onVehiclesUpdated() {
    final now = DateTime.now();
    final vehicles = GTFSService.vehiclesNotifier.value;

    final newPositions = <String, LatLng>{};

    for (final routeId in widget.vehiclePositions) {
      final vehiclesForRoute = vehicles[routeId];

      if (vehiclesForRoute == null) continue;

      for (final v in vehiclesForRoute) {
        final id = v.vehicle.id;

        newPositions[id] = LatLng(
          v.position.latitude,
          v.position.longitude,
        );
      }
    }

    // How long since the previous GTFS update?
    final elapsed = _lastUpdate == null
        ? const Duration(seconds: 10)
        : now.difference(_lastUpdate!);

    _lastUpdate = now;

    // Don't let an unusually long network delay create a crazy-long animation.
    final animationDuration = Duration(
      milliseconds: elapsed.inMilliseconds.clamp(
        1000,
        30000,
      ),
    );

    // Update each vehicle.
    for (final entry in newPositions.entries) {
      final id = entry.key;
      final newPosition = entry.value;

      if (_fromPositions.containsKey(id)) {
        // IMPORTANT:
        // Start from where the marker is RIGHT NOW,
        // not from the previous GPS coordinate.
        _fromPositions[id] = _interpolatedPosition(id);
      } else {
        // First time we've seen this vehicle.
        _fromPositions[id] = newPosition;
      }

      _toPositions[id] = newPosition;
    }

    // Remove vehicles that disappeared from the feed.
    final activeIds = newPositions.keys.toSet();

    _fromPositions.removeWhere(
      (id, _) => !activeIds.contains(id),
    );

    _toPositions.removeWhere(
      (id, _) => !activeIds.contains(id),
    );

    // Animate using the ACTUAL time between GTFS updates.
    _controller
      ..stop()
      ..duration = animationDuration
      ..reset()
      ..forward();
  }

  LatLng _interpolatedPosition(String id) {
    final from = _fromPositions[id];
    final to = _toPositions[id];

    if (from == null && to == null) {
      return const LatLng(0, 0);
    }

    if (from == null) {
      return to!;
    }

    if (to == null) {
      return from;
    }

    final t = Curves.linear.transform(
      _controller.value,
    );

    return LatLng(
      from.latitude +
          (to.latitude - from.latitude) * t,
      from.longitude +
          (to.longitude - from.longitude) * t,
    );
  }

  @override
  Widget build(BuildContext context) {
    final vehicles = GTFSService.vehiclesNotifier.value;

    final markers = <Marker>[];

    for (final routeId in widget.vehiclePositions) {
      final vehiclesForRoute = vehicles[routeId];

      if (vehiclesForRoute == null) continue;

      final route = GTFSService.routes.firstWhere(
        (r) => r.routeId == routeId,
      );

      for (final v in vehiclesForRoute) {
        final vehicleId = v.vehicle.id;

        final position = _interpolatedPosition(vehicleId);

        markers.add(
          Marker(
            key: ValueKey(vehicleId),
            width: 70,
            height: 70,
            point: position,
            child: VehicleMarker(
              routeNumber: route.routeShortName!,
              color: colorFromHex(route.routeColor!),
              bearing: v.position.bearing > -1 ? v.position.bearing : null,
              speed: v.position.speed,
              vehicleId: vehicleId,
            ),
          ),
        );
      }
    }

    return MarkerLayer(
      markers: markers,
    );
  }
}