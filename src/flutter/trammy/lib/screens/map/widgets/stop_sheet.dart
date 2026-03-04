import 'package:flutter/material.dart';
import 'package:trammy/controllers/map_screen_controller.dart';
import 'package:trammy/models/gtfs/stop.dart';
import 'package:trammy/screens/map/widgets/arrival_card.dart';
import 'package:trammy/services/common.dart';
import 'package:trammy/services/gtfs_service.dart';

class StopSheet extends StatefulWidget {
  final GTFSStopRouteInfo stop;
  final MapScreenController mapScreenController;

  StopSheet({super.key, required this.stop, required this.mapScreenController});

  @override
  State<StatefulWidget> createState() => StopSheetState();
}

class StopSheetState extends State<StopSheet> { 
  var isLoading = true;
  Map<StopInfoKey, List<DateTime>> updates = { };

  @override
  Widget build(BuildContext context) {
 // Fetch data only once
    if (isLoading) {
      widget.mapScreenController.getUpdatesForStop(widget.stop).then((result) {
        setState(() {
          updates = result;
          isLoading = false;
        });
      });
    }

    return DraggableScrollableSheet(
      initialChildSize: 0.5,
      minChildSize: 0.35,
      maxChildSize: 0.75,
      expand: false,
      builder: (context, scrollController) {
        return Container(
          decoration: BoxDecoration(color:  Theme.of(context).colorScheme.surfaceContainerHigh, borderRadius: BorderRadius.vertical(top: Radius.circular(16))),
          child: Padding(
            padding: const EdgeInsets.all(16),
            child: isLoading
                ? Column(
                    mainAxisSize: MainAxisSize.min,
                    children: [
                      Row(
                        children: [
                          Container(
                            width: 36,
                            height: 36,
                            decoration: BoxDecoration(color: colorFromHex(widget.stop.getDominantColor()!), shape: BoxShape.circle),
                            child: Icon(GTFSService.getStopIcon(widget.stop.getDominantType()!),color: Colors.white, size: 20),
                          ),
                          const SizedBox(width: 8),
                          Expanded(
                            child: Text("${widget.stop.stopName} (${widget.stop.stopCode})", style: const TextStyle(fontSize: 18, fontWeight: FontWeight.bold))
                          ),
                        ],
                      ),
                      const SizedBox(height: 24),
                      const Center(child: CircularProgressIndicator()),
                    ],
                  )
                : _buildArrivalsContent(widget.stop, updates, DateTime.now(), scrollController)
          ),
        );
      },
    );
  }

  
  Widget _buildArrivalsContent(
    GTFSStopRouteInfo stop,
    Map<StopInfoKey, List<DateTime>> updates,
    DateTime now,
    ScrollController scrollController,
  ) {
    return SingleChildScrollView(
      controller: scrollController,
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              Container(
                width: 36,
                height: 36,
                decoration: BoxDecoration(color: colorFromHex(stop.getDominantColor()!), shape: BoxShape.circle),
                child: Icon(GTFSService.getStopIcon(stop.getDominantType()!), color: Colors.white, size: 20)
              ),
              const SizedBox(width: 8),
              Text("${stop.stopName} (${stop.stopCode})", style: const TextStyle(fontSize: 18, fontWeight: FontWeight.bold)),
            ],
          ),
          const SizedBox(height: 12),

          if (updates.isEmpty)
            const Text("Няма пристигащи скоро превозни средства.")
          else
            ...buildArrivalsList(updates, now),
        ],
      ),
    );
  }

  List<Widget> buildArrivalsList(
    Map<StopInfoKey, List<DateTime>> updates,
    DateTime now,
  ) {
    final sortedUpdates = updates.entries.toList()
      ..sort((a, b) {
        final vehicleCompare = a.key.route.routeType!.compareTo(
          b.key.route.routeType!,
        );

        if (vehicleCompare != 0) return vehicleCompare;

        int parseLine(String line) => int.tryParse(line) ?? 1337;

        return parseLine(
          a.key.route.routeShortName!,
        ).compareTo(parseLine(b.key.route.routeShortName!));
      });

    return sortedUpdates.map((entry) {
      if (entry.value.isEmpty) {
        return const SizedBox.shrink();
      }

      final minutes = entry.value
          .map((date) {
            var diff = date.difference(now).inMinutes;
            if (diff < 0) diff = 0;
            return "$diff мин";
          })
          .take(3)
          .toList();

      return ArrivalCard(entry: entry, minutes: minutes);
    }).toList();
  }

}
