import 'dart:math' as math;
import 'package:flutter/material.dart';

class VehicleMarker extends StatelessWidget {
  final String routeNumber;
  final Color color;
  final double bearing;
  final double? speed;
  final String vehicleId;

  const VehicleMarker({
    super.key,
    required this.routeNumber,
    required this.color,
    required this.bearing,
    required this.vehicleId,
    this.speed,
  });

  String get speedText => speed != null 
      ? '${(speed! * 3.6).toStringAsFixed(1)} km/h' 
      : 'Stationary';

  @override
  Widget build(BuildContext context) {
    print('Building vehicle marker for ${vehicleId}, bearing ${bearing}, speed ${speed}');
    return Tooltip(
      message: 'ID: $vehicleId\nSpeed: $speedText',
      triggerMode: TooltipTriggerMode.tap,
      child: Container(
        padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
        decoration: BoxDecoration(
          color: color,
          borderRadius: BorderRadius.circular(6),
          border: Border.all(color: Colors.white, width: 2),
          boxShadow: const [
            BoxShadow(
              color: Colors.black26,
              blurRadius: 4,
              offset: Offset(0, 2),
            ),
          ],
        ),
        child: Row(
          mainAxisSize: MainAxisSize.min, // Shrinks to fit content
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Text(
              routeNumber,
              style: const TextStyle(
                color: Colors.white,
                fontSize: 12,
                fontWeight: FontWeight.bold,
              ),
            ),
            if (bearing != 0)
            const SizedBox(width: 6), // Space between number and arrow
            if (bearing != 0)
            Transform.rotate(
              // Convert degrees to radians for Flutter
              angle: bearing * (math.pi / 180),
              child: const Icon(
                Icons.navigation,
                size: 14,
                color: Colors.white, // Arrow is now white inside the box
              ),
            ),
          ],
        ),
      ),
    );
  }
}