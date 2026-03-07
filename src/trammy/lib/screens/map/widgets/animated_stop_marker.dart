import 'package:flutter/material.dart';
import 'package:trammy/models/gtfs/stop.dart';
class AnimatedStopMarker extends StatefulWidget {
  final double radius;
  final Color fillColor;
  final Color borderColor;
  final VoidCallback onTap;
  final GTFSStopRouteInfo stop; // optional, only needed if bubble is shown
  final bool isSelected;

  AnimatedStopMarker({
    super.key,
    required this.radius,
    required this.fillColor,
    required this.borderColor,
    required this.onTap,
    required this.stop, 
    required this.isSelected,
  });

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
      child: Stack(
        alignment: Alignment.center,
        clipBehavior: Clip.none,
        children: [
          // Bubble above the circle
          if (widget.isSelected && widget.stop.stopName != null && widget.stop.stopCode != null)
            Positioned(
              bottom: widget.radius + 8,
              child: AnimatedOpacity(
                duration: const Duration(milliseconds: 200),
                opacity: 0.8,
                child: Material(
                  elevation: 4,
                  borderRadius: BorderRadius.circular(16),

                  color: Colors.white,
                    child: Padding(
                      padding: const EdgeInsets.symmetric(horizontal: 18, vertical: 14),
                      child: Text(
                          "${widget.stop.stopName!} (${widget.stop.stopCode!})",
                          style: Theme.of(context).textTheme.bodyMedium?.copyWith(
                              fontWeight: FontWeight.w600,
                          ),
                        )
                        ),
                  ),
                ),
              ),            

          // The animated circle
          TweenAnimationBuilder<double>(
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
        ],
      ),
    );
  }
}