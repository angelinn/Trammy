import 'package:flutter/material.dart';
import 'package:flutter_map/flutter_map.dart';
import 'package:gtfs_realtime_bindings/gtfs_realtime_bindings.dart';
import 'package:latlong2/latlong.dart';
import 'package:trammy/screens/map/widgets/vehicle_market.dart';
import 'package:trammy/services/common.dart';
import 'package:trammy/services/gtfs_service.dart';

class AnimatedVehiclesLayer extends StatefulWidget {
  final String? selectedRouteId;

  const AnimatedVehiclesLayer({
    super.key,
    this.selectedRouteId
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
  Map<String, List<VehiclePosition>> vehiclePositions = {};

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

    vehiclePositions = Map.from(GTFSService.vehiclesNotifier.value);

    final newPositions = <String, LatLng>{};

    for (final singleRouteVehiclePositions in vehiclePositions.values) {
      for (final vehicle in singleRouteVehiclePositions) {
        newPositions[vehicle.vehicle.id] = LatLng(
          vehicle.position.latitude,
          vehicle.position.longitude,
        );
      }
    }

    // How long since the previous GTFS update?
    final elapsed = _lastUpdate == null
        ? const Duration(seconds: 10)
        : now.difference(_lastUpdate!);

    _lastUpdate = now;

    // defend against phone sleep or screen off
    if (elapsed > const Duration(seconds: 12)) {
      for (final entry in newPositions.entries) {
        _fromPositions[entry.key] = entry.value;
        _toPositions[entry.key] = entry.value;
      }

      _controller.stop();

      setState(() {
        vehiclePositions = GTFSService.vehiclesNotifier.value;
      });

      return;
    }

    // Don't let an unusually long network delay create a crazy-long animation.
    final animationDuration = Duration(
      milliseconds: elapsed.inMilliseconds.clamp(
        1000,
        12000,
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
    final markers = <Marker>[];

    for (final singleRouteVehiclePositions in vehiclePositions.values) {
      if (widget.selectedRouteId != null && !singleRouteVehiclePositions.any((v) => v.trip.routeId == widget.selectedRouteId)) {
        continue;
      }

      final route = GTFSService.routes.firstWhere(
        (r) => r.routeId == singleRouteVehiclePositions.first.trip.routeId,
      );

      for (final vehiclePosition in singleRouteVehiclePositions) {
        final vehicleId = vehiclePosition.vehicle.id;

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
              bearing: vehiclePosition.position.bearing > -1 ? vehiclePosition.position.bearing : null,
              speed: vehiclePosition.position.speed,
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