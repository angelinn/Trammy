import 'package:flutter/material.dart';

class PulsingUserMarker extends StatefulWidget {
  final bool animatePop;

  const PulsingUserMarker({
    super.key,
    this.animatePop = false,
  });

  @override
  State<PulsingUserMarker> createState() => _PulsingUserMarkerState();
}

class _PulsingUserMarkerState extends State<PulsingUserMarker>
    with TickerProviderStateMixin {
  late final AnimationController _pulseController;
  late final AnimationController _popController;

  @override
  void initState() {
    super.initState();

    _pulseController = AnimationController(
      vsync: this,
      duration: const Duration(seconds: 2),
    )..repeat();

    _popController = AnimationController(
      vsync: this,
      duration: const Duration(milliseconds: 250),
    );
  }

  @override
  void didUpdateWidget(PulsingUserMarker oldWidget) {
    super.didUpdateWidget(oldWidget);

    if (widget.animatePop && !oldWidget.animatePop) {
      _popController.forward(from: 0);
    }
  }

  @override
  void dispose() {
    _pulseController.dispose();
    _popController.dispose();
    super.dispose();
  }

Widget build(BuildContext context) {
  return SizedBox(
    width: 80,
    height: 80,
    child: AnimatedBuilder(
      animation: Listenable.merge([_pulseController, _popController]),
      builder: (context, child) {
        final pulse = _pulseController.value;
        final popScale = 1 + (_popController.value * 0.25);

        return Transform.scale(
          scale: popScale,
          child: Stack(
            alignment: Alignment.center,
            children: [
              // Pulsing circle
              Container(
                width: 60 * pulse,
                height: 60 * pulse,
                decoration: BoxDecoration(
                  shape: BoxShape.circle,
                  color: Colors.blue.withOpacity(
                    0.25 * (1 - pulse),
                  ),
                ),
              ),

              // Center dot
              Container(
                width: 18,
                height: 18,
                decoration: BoxDecoration(
                  shape: BoxShape.circle,
                  color: Colors.blue,
                  border: Border.all(color: Colors.white, width: 3),
                  boxShadow: const [
                    BoxShadow(
                      color: Colors.black26,
                      blurRadius: 6,
                    ),
                  ],
                ),
              ),
            ],
          ),
        );
      },
    ),
  );
}
  }
