import 'dart:math' as math;
import 'package:flutter/material.dart';

class VehicleMarker extends StatefulWidget {
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

  @override
  State<VehicleMarker> createState() => _VehicleMarkerState();
}

class _VehicleMarkerState extends State<VehicleMarker> {
  late double _targetContinuousBearing;

  @override
  void initState() {
    super.initState();
    _targetContinuousBearing = widget.bearing ?? 0;
  }

  @override
  void didUpdateWidget(VehicleMarker oldWidget) {
    super.didUpdateWidget(oldWidget);
    if (widget.bearing != null && widget.bearing != oldWidget.bearing) {
      // Calculate shortest path around 360 degrees to prevent full spins across North (0°)
      double delta = (widget.bearing! - _targetContinuousBearing) % 360;
      if (delta > 180) delta -= 360;
      if (delta < -180) delta += 360;
      _targetContinuousBearing += delta;
    }
  }

  String get speedText =>
      widget.speed != null ? '${(widget.speed!).toStringAsFixed(1)} km/h' : 'Stationary';

  @override
  Widget build(BuildContext context) {
    return Tooltip(
      message: 'ID: ${widget.vehicleId}\nSpeed: $speedText',
      triggerMode: TooltipTriggerMode.tap,
      child: SizedBox(
        width: widget.size,
        height: widget.size,
        child: Stack(
          alignment: Alignment.center,
          children: [
            
            // 1. BACKGROUND LAYER: Smoothly animates angle changes
            TweenAnimationBuilder<double>(
              tween: Tween<double>(end: _targetContinuousBearing),
              duration: const Duration(milliseconds: 600), // Adjust animation speed here
              curve: Curves.easeOutCubic, // Feels natural for vehicle motion
              builder: (context, animatedBearing, child) {
                return Transform.rotate(
                  angle: animatedBearing * (math.pi / 180),
                  child: child,
                );
              },
              child: CustomPaint(
                size: Size(widget.size, widget.size),
                painter: _VehicleMarkerPainter(
                  color: widget.color,
                  hasBearing: widget.bearing != null,
                ),
              ),
            ),

            // 2. TEXT LAYER: Stays upright continuously
            SizedBox(
              width: widget.size * 0.55,
              height: widget.size * 0.55,
              child: Center(
                child: FittedBox(
                  fit: BoxFit.scaleDown,
                  child: Text(
                    widget.routeNumber,
                    style: const TextStyle(
                      color: Colors.white,
                      fontWeight: FontWeight.w900,
                      fontSize: 12,
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

class _VehicleMarkerPainter extends CustomPainter {
  final Color color;
  final bool hasBearing;

  _VehicleMarkerPainter({required this.color, required this.hasBearing});

  @override
  void paint(Canvas canvas, Size size) {
    final center = Offset(size.width / 2, size.height / 2);
    final radius = size.width * 0.32;

    final circlePath = Path()
      ..addOval(Rect.fromCircle(center: center, radius: radius));

    Path finalPath = circlePath;

    if (hasBearing) {
      final arrowPath = Path();
      final arrowTip = Offset(center.dx, size.height * 0.05);
      final arrowBaseLeft = Offset(center.dx - radius * 0.6, center.dy - radius * 0.4);
      final arrowBaseRight = Offset(center.dx + radius * 0.6, center.dy - radius * 0.4);

      arrowPath.moveTo(arrowTip.dx, arrowTip.dy);
      arrowPath.lineTo(arrowBaseRight.dx, arrowBaseRight.dy);
      arrowPath.lineTo(arrowBaseLeft.dx, arrowBaseLeft.dy);
      arrowPath.close();

      finalPath = Path.combine(PathOperation.union, circlePath, arrowPath);
    }

    canvas.drawShadow(finalPath, Colors.black, 4.0, false);

    final fillPaint = Paint()
      ..color = color
      ..style = PaintingStyle.fill;
    canvas.drawPath(finalPath, fillPaint);

    final borderPaint = Paint()
      ..color = Colors.white
      ..strokeWidth = size.width * 0.04
      ..strokeJoin = StrokeJoin.round
      ..style = PaintingStyle.stroke;
    canvas.drawPath(finalPath, borderPaint);
  }

  @override
  bool shouldRepaint(covariant _VehicleMarkerPainter oldDelegate) {
    return oldDelegate.color != color || oldDelegate.hasBearing != hasBearing;
  }
}