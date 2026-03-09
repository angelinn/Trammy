import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:trammy/controllers/map_screen_controller.dart';
import 'package:trammy/db/favourites_repository.dart';
import 'package:trammy/db/user_db_service.dart';
import 'package:trammy/models/favourite.dart';
import 'package:trammy/models/gtfs/stop.dart';
import 'package:trammy/screens/map/widgets/arrival_card.dart';
import 'package:trammy/services/common.dart';
import 'package:trammy/services/gtfs_service.dart';

class StopSheet extends StatefulWidget {
  final GTFSStopRouteInfo stop;
  final void Function(Set<String> routeIds)? onShowVehicles;

  const StopSheet({super.key, required this.stop, this.onShowVehicles});

  @override
  State<StopSheet> createState() => StopSheetState();
}

class StopSheetState extends State<StopSheet> {
  bool isLoading = true;
  bool isFavorite = false;
  Map<StopInfoKey, List<ArrivalEntry>> updates = {};

  @override
  void initState() {
    super.initState();
    _fetchUpdates();
    _checkFavoriteStatus();
  }

  Future<void> _checkFavoriteStatus() async {
    final status = await UserDbService.isFavorite(widget.stop.stopCode!);
    if (mounted) {
      setState(() => isFavorite = status);
    }
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
  final transportType = GTFSService.getDominantType(widget.stop)!;
  final transportColor = colorFromHex(GTFSService.getDominantColor(widget.stop)!);

  return Column(
    crossAxisAlignment: CrossAxisAlignment.start,
    children: [
      // --- TOP ROW: Transport icon + stop name/code + favorite star ---
      Row(
        crossAxisAlignment: CrossAxisAlignment.center,
        children: [
          // Transport icon badge
          Container(
            width: 36,
            height: 36,
            decoration: BoxDecoration(
              color: transportColor,
              shape: BoxShape.circle,
            ),
            child: Icon(
              GTFSService.getStopIcon(transportType),
              color: Colors.white,
              size: 20,
            ),
          ),
          const SizedBox(width: 12),

          // Stop name and code
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  widget.stop.stopName ?? "",
                  style: const TextStyle(fontSize: 18, fontWeight: FontWeight.bold),
                  maxLines: 1,
                  overflow: TextOverflow.ellipsis,
                ),
                Text(
                  "Код: ${widget.stop.stopCode}",
                  style: TextStyle(
                    fontSize: 13,
                    color: Theme.of(context).colorScheme.onSurfaceVariant,
                  ),
                ),
              ],
            ),
          ),

          // Favorite button (AnimatedScale + AnimatedSwitcher)
          AnimatedScale(
            scale: isFavorite ? 1.2 : 1.0,
            duration: const Duration(milliseconds: 200),
            curve: Curves.easeOutBack,
            child: AnimatedSwitcher(
              duration: const Duration(milliseconds: 250),
              transitionBuilder: (child, animation) =>
                  ScaleTransition(scale: animation, child: child),
              child: IconButton(
                key: ValueKey<bool>(isFavorite),
                onPressed: () async {
                  final fav = FavoriteStop(
                    stopCode: widget.stop.stopCode!,
                    stopName: widget.stop.stopName!,
                    routeType: transportType.value,
                  );

                  await FavouritesRepository.instance.toggle(fav);

                  if (!mounted) return;
                  setState(() => isFavorite = !isFavorite);

                  HapticFeedback.lightImpact();

                  ScaffoldMessenger.of(context).showSnackBar(
                    SnackBar(
                      content: Text(isFavorite
                          ? '${widget.stop.stopName} е добавена в любими'
                          : '${widget.stop.stopName} е премахната от любими'),
                    ),
                  );
                },
                icon: Icon(
                  isFavorite ? Icons.star_rounded : Icons.star_outline_rounded,
                  color: isFavorite
                      ? Theme.of(context).colorScheme.tertiary
                      : Theme.of(context).colorScheme.outline,
                  size: 28,
                ),
              ),
            ),
          ),
        ],
      ),

      const SizedBox(height: 12),

      // --- VEHICLES BUTTON BELOW: Tonal IconButton ---
      FilledButton.icon(
          icon: const Icon(Icons.directions_transit),
          label: const Text("На живо"),
          onPressed: () {
            final routes = widget.stop.routeIds!.split(',').toSet();
            // TODO: Call callback to show vehicles on map
            widget.onShowVehicles?.call(routes);
            Navigator.pop(context);
          },
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