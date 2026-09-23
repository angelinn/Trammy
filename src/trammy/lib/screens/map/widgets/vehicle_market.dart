import 'dart:math' as math;
import 'package:flutter/material.dart';

class VehicleMarker extends StatelessWidget {
  final String routeNumber;
  final Color color;
  final double? bearing;
  final double? speed;
  final String vehicleId;
  final double size; 

  const VehicleMarker({
    super.key,
    required this.routeNumber,
    required this.color,
    this.bearing,
    required this.vehicleId,
    this.speed,
    this.size = 55.0, 
  });

  String get speedText => speed != null ? '${(speed!).toStringAsFixed(1)} km/h' : 'Stationary';

  @override
  Widget build(BuildContext context) {
    return Tooltip(
      message: 'ID: $vehicleId\nSpeed: $speedText',
      triggerMode: TooltipTriggerMode.tap,
      child: SizedBox(
        width: size,
        height: size,
        child: Stack(
          alignment: Alignment.center,
          children: [
            // 1. BACKGROUND LAYER: Rotates based on bearing
            Transform.rotate(
              angle: (bearing ?? 0) * (math.pi / 180),
              child: CustomPaint(
                size: Size(size, size),
                painter: VehicleMarkerPainter(
                  color: color,
                  hasBearing: bearing != null,
                ),
              ),
            ),

            // 2. TEXT LAYER: Stays perfectly upright
            SizedBox(
              // Constrain text to the inner circle radius
              width: size * 0.55,
              height: size * 0.55,
              child: Center(
                child: FittedBox(
                  fit: BoxFit.scaleDown,
                  child: Text(
                    routeNumber,
                    style: const TextStyle(
                      color: Colors.white,
                      fontWeight: FontWeight.w900, // Heavy weight for transit readability
                      fontSize: 12, // High base size, FittedBox will scale it down if needed
                      letterSpacing: -0.5,
                    ),
                  ),
                ),
              ),
            ),
            
          ],
        ),
      ),
    );
  }
}

/// A Custom Painter that draws a seamless circle with a directional pointer
class VehicleMarkerPainter extends CustomPainter {
  final Color color;
  final bool hasBearing;

  VehicleMarkerPainter({required this.color, required this.hasBearing});

  @override
  void paint(Canvas canvas, Size size) {
    final center = Offset(size.width / 2, size.height / 2);
    // Radius of the main circle leaves room for the arrow and drop shadow
    final radius = size.width * 0.32; 

    // Define the base circle
    final circlePath = Path()
      ..addOval(Rect.fromCircle(center: center, radius: radius));

    Path finalPath = circlePath;

    if (hasBearing) {
      final arrowPath = Path();
      
      // Tip of the arrow (pointing UP towards North/0 degrees)
      final arrowTip = Offset(center.dx, size.height * 0.05);
      
      // The base corners of the arrow triangle (overlapping the top of the circle)
      final arrowBaseLeft = Offset(center.dx - radius * 0.6, center.dy - radius * 0.4);
      final arrowBaseRight = Offset(center.dx + radius * 0.6, center.dy - radius * 0.4);

      arrowPath.moveTo(arrowTip.dx, arrowTip.dy);
      arrowPath.lineTo(arrowBaseRight.dx, arrowBaseRight.dy);
      arrowPath.lineTo(arrowBaseLeft.dx, arrowBaseLeft.dy);
      arrowPath.close();

      // Merge the circle and arrow into one seamless, unified shape
      finalPath = Path.combine(PathOperation.union, circlePath, arrowPath);
    }

    // 1. Draw a clean drop shadow
    canvas.drawShadow(finalPath, Colors.black, 4.0, false);

    // 2. Fill the shape with the route color
    final fillPaint = Paint()
      ..color = color
      ..style = PaintingStyle.fill;
    canvas.drawPath(finalPath, fillPaint);

    // 3. Draw the seamless white border around the merged shape
    final borderPaint = Paint()
      ..color = Colors.white
      ..strokeWidth = size.width * 0.04 // Dynamic border thickness
      ..strokeJoin = StrokeJoin.round // Smooth, rounded inner corners
      ..style = PaintingStyle.stroke;
    canvas.drawPath(finalPath, borderPaint);
  }

  @override
  bool shouldRepaint(covariant VehicleMarkerPainter oldDelegate) {
    return oldDelegate.color != color || oldDelegate.hasBearing != hasBearing;
  }
}