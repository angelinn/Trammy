import 'package:flutter/material.dart';

class AnimatedStopMarker extends StatefulWidget {
  final double radius;
    final Color fillColor;
    final Color borderColor;
    final VoidCallback onTap;

    AnimatedStopMarker({super.key, required this.radius, required this.fillColor, required this.borderColor, required this.onTap});

  @override
  State<StatefulWidget> createState() => AnimatedStopMarkerState();
}

class AnimatedStopMarkerState extends State<AnimatedStopMarker> {
  
  bool tapped = false;

  void handleTap() async {
    setState(() => tapped = true);
    await Future.delayed(const Duration(milliseconds: 140));
    setState(() => tapped = false);
    widget.onTap();
  }

  @override
  Widget build(BuildContext context) {
    
    return GestureDetector(
          onTap: handleTap,
          child: TweenAnimationBuilder<double>(
            tween: Tween(begin: 1.0, end: tapped ? 1.5 : 1.0),
            duration: const Duration(milliseconds: 140),
            curve: Curves.easeOut,
            builder: (context, scale, child) {
              return Transform.scale(scale: scale, child: child);
            },
            child: Material(
              color: Colors.transparent,
              shape: const CircleBorder(),
              child: Container(
                width: widget.radius,
                height: widget.radius,
                decoration: BoxDecoration(
                  shape: BoxShape.circle,
                  color: widget.fillColor,
                  border: Border.all(color: widget.borderColor, width: 1),
                ),
              ),
            ),
          ),
        ); 
  }
}