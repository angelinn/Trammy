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
    this.selectedRouteId,
  });

  @override
  State<AnimatedVehiclesLayer> createState() =>
      _AnimatedVehiclesLayerState();
}

class _AnimatedVehiclesLayerState extends State<AnimatedVehiclesLayer>
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
    );

    GTFSService.vehiclesNotifier.addListener(_onVehiclesUpdated);

    // Load vehicles that already exist.
    _onVehiclesUpdated();
  }

  @override
  void dispose() {
    GTFSService.vehiclesNotifier.removeListener(_onVehiclesUpdated);
    _controller.dispose();
    super.dispose();
  }

  bool _isValidLatLng(LatLng position) {
    return position.latitude.isFinite &&
        position.longitude.isFinite &&
        position.latitude >= -90 &&
        position.latitude <= 90 &&
        position.longitude >= -180 &&
        position.longitude <= 180;
  }

  void _onVehiclesUpdated() {
    if (!mounted) return;

    final now = DateTime.now();

    final newVehiclePositions =
        Map<String, List<VehiclePosition>>.from(
      GTFSService.vehiclesNotifier.value,
    );

    // IDs that are actually present in the feed, regardless of whether
    // their current coordinate is valid.
    final feedVehicleIds = <String>{};

    // Only valid coordinates are allowed into the animation state.
    final validNewPositions = <String, LatLng>{};

    for (final routeVehicles in newVehiclePositions.values) {
      for (final vehicle in routeVehicles) {
        final id = vehicle.vehicle.id;

        if (id.isEmpty) {
          continue;
        }

        feedVehicleIds.add(id);

        final position = LatLng(
          vehicle.position.latitude,
          vehicle.position.longitude,
        );

        if (!_isValidLatLng(position)) {
          debugPrint(
            'Ignoring invalid position for vehicle $id: '
            '${vehicle.position.latitude}, '
            '${vehicle.position.longitude}',
          );
          continue;
        }

        validNewPositions[id] = position;
      }
    }

    final elapsed = _lastUpdate == null
        ? const Duration(seconds: 10)
        : now.difference(_lastUpdate!);

    _lastUpdate = now;

    // If the app was asleep / backgrounded for a while, snap everything
    // directly to the latest valid coordinates.
    if (elapsed > const Duration(seconds: 30)) {
      for (final entry in validNewPositions.entries) {
        _fromPositions[entry.key] = entry.value;
        _toPositions[entry.key] = entry.value;
      }

      // Vehicles that vanished from the feed should be removed.
      _fromPositions.removeWhere(
        (id, _) => !feedVehicleIds.contains(id),
      );
      _toPositions.removeWhere(
        (id, _) => !feedVehicleIds.contains(id),
      );

      _controller
        ..stop()
        ..value = 0.0;

      setState(() {
        vehiclePositions = newVehiclePositions;
      });

      return;
    }

    final animationDuration = Duration(
      milliseconds: elapsed.inMilliseconds.clamp(1000, 20000).toInt(),
    );

    for (final entry in validNewPositions.entries) {
      final id = entry.key;
      final newPosition = entry.value;

      final oldTo = _toPositions[id];

      if (oldTo == null) {
        // First valid observation.
        _fromPositions[id] = newPosition;
      } else {
        // Start from where the marker currently is.
        final current = _interpolatedPosition(id);

        if (_isValidLatLng(current)) {
          _fromPositions[id] = current;
        } else {
          // Defensive fallback.
          _fromPositions[id] = oldTo;
        }
      }

      _toPositions[id] = newPosition;
    }

    // Remove vehicles which no longer exist in the feed.
    _fromPositions.removeWhere(
      (id, _) => !feedVehicleIds.contains(id),
    );
    _toPositions.removeWhere(
      (id, _) => !feedVehicleIds.contains(id),
    );

    setState(() {
      vehiclePositions = newVehiclePositions;
    });

    _controller
      ..stop()
      ..duration = animationDuration
      ..reset()
      ..forward();
  }

  LatLng? _interpolatedPositionOrNull(String id) {
    final from = _fromPositions[id];
    final to = _toPositions[id];

    if (from == null && to == null) {
      return null;
    }

    if (from == null) {
      return _isValidLatLng(to!) ? to : null;
    }

    if (to == null) {
      return _isValidLatLng(from) ? from : null;
    }

    if (!_isValidLatLng(from) || !_isValidLatLng(to)) {
      return null;
    }

    final t = Curves.linear.transform(_controller.value);

    final position = LatLng(
      from.latitude + (to.latitude - from.latitude) * t,
      from.longitude + (to.longitude - from.longitude) * t,
    );

    return _isValidLatLng(position) ? position : null;
  }

  LatLng _interpolatedPosition(String id) {
    return _interpolatedPositionOrNull(id) ??
        const LatLng(0, 0);
  }

  @override
  Widget build(BuildContext context) {
    return AnimatedBuilder(
      animation: _controller,
      builder: (context, _) {
        final camera = MapCamera.of(context);

        if (camera.zoom < 14) {
          return const MarkerLayer(markers: []);
        }

        final bounds = camera.visibleBounds;
        final markers = <Marker>[];

        for (final routeVehicles in vehiclePositions.values) {
          if (routeVehicles.isEmpty) {
            continue;
          }

          if (widget.selectedRouteId != null &&
              !routeVehicles.any(
                (v) => v.trip.routeId == widget.selectedRouteId,
              )) {
            continue;
          }

          final routeId = routeVehicles.first.trip.routeId;

          final route = GTFSService.routesById[routeId];
          if (route == null) {
            continue;
          }

          for (final vehiclePosition in routeVehicles) {
            final vehicleId = vehiclePosition.vehicle.id;

            if (vehicleId.isEmpty) {
              continue;
            }

            final from = _fromPositions[vehicleId];
            final to = _toPositions[vehicleId];

            // Don't render vehicles for which we have never had
            // a valid position.
            if (from == null && to == null) {
              continue;
            }

            // Cheap culling before interpolation.
            final isFromInView =
                from != null && _isValidLatLng(from) && bounds.contains(from);

            final isToInView =
                to != null && _isValidLatLng(to) && bounds.contains(to);

            if (!isFromInView && !isToInView) {
              continue;
            }

            final position =
                _interpolatedPositionOrNull(vehicleId);

            // Never give flutter_map an invalid coordinate.
            if (position == null) {
              continue;
            }

            if (!bounds.contains(position)) {
              continue;
            }

            markers.add(
              Marker(
                //key: ValueKey(vehicleId),
                width: 70,
                height: 70,
                point: position,
                child: VehicleMarker(
                  key: ValueKey(vehicleId),
                  routeNumber: route.routeShortName ?? '',
                  color: colorFromHex(route.routeColor ?? '000000'),
                  bearing: vehiclePosition.position.bearing.isFinite &&
                          vehiclePosition.position.bearing > 0
                      ? vehiclePosition.position.bearing
                      : null,
                  speed: vehiclePosition.position.speed.isFinite
                      ? vehiclePosition.position.speed
                      : null,
                  vehicleId: vehicleId,
                ),
              ),
            );
          }
        }

        return MarkerLayer(markers: markers);
      },
    );
  }
}