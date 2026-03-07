import 'package:flutter/material.dart';
import 'package:trammy/db/user_db_service.dart';
import 'package:trammy/models/favourite.dart';
import 'package:trammy/models/gtfs/stop.dart';
import 'package:trammy/services/gtfs_service.dart';
import 'package:trammy/services/common.dart';

class FavouritesScreen extends StatefulWidget { 
  final void Function(FavoriteStop) onFavouriteSelected;
  const FavouritesScreen({super.key, required this.onFavouriteSelected});

  @override
  State<FavouritesScreen> createState() => FavouritesScreenState();
}

class FavouritesScreenState extends State<FavouritesScreen> {
  List<FavoriteStop> favorites = [];
  bool isLoading = true;

  @override
  void initState() {
    super.initState();
    _refreshFavorites();
  }

  Future<void> _refreshFavorites() async {
    final data = await UserDbService.getFavorites();
    setState(() {
      favorites = data;
      isLoading = false;
    });
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Любими спирки')),
      body: CustomScrollView(
        slivers: [
          if (isLoading)
            const SliverFillRemaining(
              child: Center(child: CircularProgressIndicator()),
            )
          else if (favorites.isEmpty)
            _buildEmptyState()
          else
            SliverPadding(
              padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
              sliver: SliverList(
                delegate: SliverChildBuilderDelegate(
                  (context, index) => _buildFavoriteItem(favorites[index]),
                  childCount: favorites.length,
                ),
              ),
            ),
        ],
      ),
    );
  }

  Widget _buildEmptyState() {
    return SliverFillRemaining(
      hasScrollBody: false,
      child: Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Icon(
              Icons.star_outline_rounded,
              size: 80,
              color: Theme.of(context).colorScheme.outlineVariant,
            ),
            const SizedBox(height: 16),
            Text(
              "Нямате добавени любими",
              style: Theme.of(context).textTheme.titleMedium?.copyWith(
                    color: Theme.of(context).colorScheme.onSurfaceVariant,
                  ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildFavoriteItem(FavoriteStop fav) {
    // Get color and icon based on routeType (0=Tram, 3=Bus, etc.)
    final TransportType type = TransportType.fromValue(fav.routeType);
    final Color transportColor = colorFromHex(GTFSService.routeColors[type]!);
    final IconData transportIcon = GTFSService.getStopIcon(type);

    return Padding(
      padding: const EdgeInsets.only(bottom: 12),
      child: Dismissible(
        key: Key(fav.stopCode),
        direction: DismissDirection.endToStart,
        onDismissed: (_) async {
          await UserDbService.toggleFavorite(fav); // Removes it
          _refreshFavorites();
        },
        background: Container(
          alignment: Alignment.centerRight,
          padding: const EdgeInsets.only(right: 20),
          decoration: BoxDecoration(
            color: Theme.of(context).colorScheme.errorContainer,
            borderRadius: BorderRadius.circular(16),
          ),
          child: Icon(Icons.delete_outline, color: Theme.of(context).colorScheme.error),
        ),
        child: Card(
          elevation: 0,
          margin: EdgeInsets.zero,
          // M3 Surface color
          color: Theme.of(context).colorScheme.surfaceContainerLow,
          shape: RoundedRectangleBorder(
            borderRadius: BorderRadius.circular(16),
            side: BorderSide(
              color: Theme.of(context).colorScheme.outlineVariant.withOpacity(0.5),
            ),
          ),
          child: ListTile(
            contentPadding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
            leading: CircleAvatar(
              backgroundColor: transportColor,
              child: Icon(transportIcon, color: Colors.white, size: 20),
            ),
            title: Text(
              fav.stopName,
              style: const TextStyle(fontWeight: FontWeight.bold),
            ),
            subtitle: Text("Код на спирка: ${fav.stopCode}"),
            trailing: Icon(
              Icons.arrow_forward_ios,
              size: 14,
              color: Theme.of(context).colorScheme.outline,
            ),
            onTap: () {
              widget.onFavouriteSelected(fav);
            },
          ),
        ),
      ),
    );
  }
}