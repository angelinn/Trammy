import 'package:flutter/material.dart';
import 'package:trammy/controllers/map_screen_controller.dart';
import 'package:trammy/models/gtfs/stop.dart';
import 'package:trammy/screens/map/widgets/arrival_card.dart';
import 'package:trammy/services/common.dart';
import 'package:trammy/services/gtfs_service.dart';

class StopSheet extends StatefulWidget {
  final GTFSStopRouteInfo stop;

  const StopSheet({super.key, required this.stop});

  @override
  State<StopSheet> createState() => StopSheetState();
}

class StopSheetState extends State<StopSheet> {
  bool isLoading = true;

  Map<StopInfoKey, List<ArrivalEntry>> updates = {};

  @override
  void initState() {
    super.initState();
    _fetchUpdates();
  }

  Future<void> _fetchUpdates() async {
    final result =
        await MapScreenController.getUpdatesForStop(widget.stop.stopCode!);

    if (!mounted) return;

    setState(() {
      updates = result;
      isLoading = false;
    });
  }

  @override
  Widget build(BuildContext context) {
    return DraggableScrollableSheet(
      initialChildSize: 0.5,
      minChildSize: 0.35,
      maxChildSize: 0.75,
      expand: false,
      builder: (context, scrollController) {
        return Container(
          decoration: BoxDecoration(
            color: Theme.of(context).colorScheme.surfaceContainerHigh,
            borderRadius: const BorderRadius.vertical(top: Radius.circular(16)),
          ),
          child: Column(
            children: [
              // 1. THE DRAG HANDLE: The builder's scrollController lives here.
              // Swiping this area will expand/shrink the bottom sheet.
              SingleChildScrollView(
                controller: scrollController,
                physics: const ClampingScrollPhysics(),
                child: Padding(
                  padding: const EdgeInsets.all(16.0),
                  child: Column(
                    children: [
                      // A standard visual cue so the user knows to drag here
                      Container(
                        width: 40,
                        height: 4,
                        margin: const EdgeInsets.only(bottom: 12),
                        decoration: BoxDecoration(
                          color: Theme.of(context).colorScheme.onSurfaceVariant.withOpacity(0.4),
                          borderRadius: BorderRadius.circular(2),
                        ),
                      ),
                      _buildHeader(),
                    ],
                  ),
                ),
              ),

              // 2. THE LIST: Independent scrolling and refreshing.
              // Swiping this area will scroll the arrivals or trigger the refresh.
              Expanded(
                child: RefreshIndicator(
                  onRefresh: _fetchUpdates,
                  child: ListView(
                    // Intentionally leaving out the scrollController here!
                    physics: const AlwaysScrollableScrollPhysics(),
                    padding: const EdgeInsets.only(left: 16, right: 16, bottom: 16),
                    children: [
                      if (isLoading)
                        const Padding(
                          padding: EdgeInsets.only(top: 24),
                          child: Center(child: CircularProgressIndicator()),
                        )
                      else
                        ..._buildArrivals(),
                    ],
                  ),
                ),
              ),
            ],
          ),
        );
      },
    );
  }

  Widget _buildHeader() {
    return Row(
      children: [
        Container(
          width: 36,
          height: 36,
          decoration: BoxDecoration(
            color: colorFromHex(GTFSService.getDominantColor(widget.stop)!),
            shape: BoxShape.circle,
          ),
          child: Icon(
            GTFSService.getStopIcon(
              GTFSService.getDominantType(widget.stop)!,
            ),
            color: Colors.white,
            size: 20,
          ),
        ),
        const SizedBox(width: 8),
        Expanded(
          child: Text(
            "${widget.stop.stopName} (${widget.stop.stopCode})",
            style: const TextStyle(fontSize: 18, fontWeight: FontWeight.bold),
          ),
        ),
      ],
    );
  }

  List<Widget> _buildArrivals() {
    final sortedUpdates = updates.entries.toList()
      ..sort((a, b) {
        final vehicleCompare =
            a.key.route.routeType!.compareTo(b.key.route.routeType!);

        if (vehicleCompare != 0) return vehicleCompare;

        int parseLine(String line) => int.tryParse(line) ?? 1337;

        return parseLine(a.key.route.routeShortName!)
            .compareTo(parseLine(b.key.route.routeShortName!));
      });

    if (sortedUpdates.isEmpty) {
      return const [
        Padding(
          padding: EdgeInsets.only(top: 16),
          child: Text("Няма пристигащи скоро превозни средства."),
        )
      ];
    }

    return sortedUpdates.map((entry) {
      if (entry.value.isEmpty) return const SizedBox.shrink();

      return ArrivalCard(
        route: entry.key.route,
        direction: entry.key.direction,
        arrivals: entry.value,
      );
    }).toList();
  }
}