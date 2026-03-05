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
  Map<StopInfoKey, List<ArrivalEntry>> updates = { };

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
                            decoration: BoxDecoration(color: colorFromHex(GTFSService.getDominantColor(widget.stop)!), shape: BoxShape.circle),
                            child: Icon(GTFSService.getStopIcon(GTFSService.getDominantType(widget.stop)!),color: Colors.white, size: 20),
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
    Map<StopInfoKey, List<ArrivalEntry>> updates,
    DateTime now,
    ScrollController scrollController,
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
                decoration: BoxDecoration(color: colorFromHex(GTFSService.getDominantColor(stop)!), shape: BoxShape.circle),
                child: Icon(GTFSService.getStopIcon(GTFSService.getDominantType(stop)!), color: Colors.white, size: 20)
              ),
              const SizedBox(width: 8),
              Text("${stop.stopName} (${stop.stopCode})", style: const TextStyle(fontSize: 18, fontWeight: FontWeight.bold)),
            ],
          ),
          const SizedBox(height: 12),

          if (sortedUpdates.isEmpty)
            const Text("Няма пристигащи скоро превозни средства.")
          else
            ...buildArrivalsList(sortedUpdates, now),
        ],
      ),
    );
  }

  List<Widget> buildArrivalsList(
    List<MapEntry<StopInfoKey, List<ArrivalEntry>>> updates,
    DateTime now,
  ) {

    return updates.map((entry) {
      if (entry.value.isEmpty) {
        return const SizedBox.shrink();
      }

      return ArrivalCard(route: entry.key.route, direction: entry.key.direction, arrivals: entry.value);
    }).toList();
  }

}
