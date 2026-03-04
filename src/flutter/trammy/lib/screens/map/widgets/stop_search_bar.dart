import 'package:flutter/material.dart';
import 'package:trammy/models/gtfs/stop.dart';

class StopSearchBar extends StatefulWidget {
  final List<GTFSStopRouteInfo> stops;
  final void Function(String) onStopSearch;

  const StopSearchBar({super.key, required this.stops, required this.onStopSearch});

  @override
  State<StatefulWidget> createState() => StopSearchBarState();
}

class StopSearchBarState extends State<StopSearchBar> {
  late TextEditingController searchController;
  late FocusNode searchFocusNode;

  @override
  void dispose() {
    searchController.dispose();
    searchFocusNode.dispose();
    super.dispose();
  }

  void onStopSearch(String value) {
    searchController.clear();
    searchFocusNode.unfocus();

    widget.onStopSearch(value);
  }

  @override
  Widget build(BuildContext context) {
    return Autocomplete<String>(
      optionsViewOpenDirection: OptionsViewOpenDirection.up,
      optionsBuilder: (TextEditingValue textEditingValue) {
        if (textEditingValue.text.isEmpty) {
          return const Iterable<String>.empty();
        }
        return widget.stops
            .where(
              (stop) =>
                  stop.stopName!.toLowerCase().contains(
                    textEditingValue.text.toLowerCase(),
                  ) ||
                  stop.stopCode!.toLowerCase().contains(
                    textEditingValue.text.toLowerCase(),
                  ),
            )
            .map((s) => "${s.stopName!} (${s.stopCode})");
      },
      onSelected: onStopSearch,
      fieldViewBuilder:
          (context, textController, focusNode, onEditingComplete) {
            searchController = textController;
            searchFocusNode = focusNode;
            return Material(
              elevation: 4,
              borderRadius: BorderRadius.circular(16),
              color: Colors.white, // make the box opaque
              child: TextField(
                controller: searchController,
                focusNode: focusNode,
                onEditingComplete: onEditingComplete,
                decoration: const InputDecoration(
                  hintText: 'Търсене на спирка...',
                  prefixIcon: Icon(Icons.search),
                  border: InputBorder.none,
                  contentPadding: EdgeInsets.symmetric(
                    vertical: 14,
                    horizontal: 16,
                  ),
                ),
              ),
            );
          },
    );
  }
}