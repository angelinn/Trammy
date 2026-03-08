import 'package:flutter/material.dart';
import 'package:trammy/db/favourites_repository.dart';
import 'package:trammy/models/favourite.dart';
import 'package:trammy/models/gtfs/stop.dart';
import 'package:trammy/services/gtfs_service.dart';
import 'package:trammy/services/common.dart';

class FavouritesScreen extends StatelessWidget {
  final void Function(FavoriteStop) onFavouriteSelected;

  const FavouritesScreen({super.key, required this.onFavouriteSelected});

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Любими спирки'),
        centerTitle: true,
        surfaceTintColor: Theme.of(context).colorScheme.surfaceTint,
      ),
      body: ValueListenableBuilder<List<FavoriteStop>>(
        valueListenable: FavouritesRepository.instance.favorites,
        builder: (context, favorites, _) {
          if (favorites.isEmpty) {
            return CustomScrollView(
              slivers: [_buildEmptyState(context)],
            );
          }

          return CustomScrollView(
            slivers: [
              SliverPadding(
                padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
                sliver: SliverList(
                  delegate: SliverChildBuilderDelegate(
                    (context, index) => _buildFavoriteItem(context, favorites[index]),
                    childCount: favorites.length,
                  ),
                ),
              ),
            ],
          );
        },
      ),
    );
  }

  Widget _buildEmptyState(BuildContext context) {
    final theme = Theme.of(context);
    return SliverFillRemaining(
      hasScrollBody: false,
      child: Center(
        child: Padding(
          padding: const EdgeInsets.symmetric(horizontal: 24),
          child: Column(
            mainAxisAlignment: MainAxisAlignment.center,
            children: [
              Icon(
                Icons.star_outline_rounded,
                size: 96,
                color: theme.colorScheme.outlineVariant,
              ),
              const SizedBox(height: 24),
              Text(
                "Нямате добавени любими",
                style: theme.textTheme.titleLarge?.copyWith(
                  color: theme.colorScheme.onSurfaceVariant,
                ),
                textAlign: TextAlign.center,
              ),
              const SizedBox(height: 8),
              Text(
                "Добавете спирки към любимите си, за да ги виждате бързо тук.",
                style: theme.textTheme.bodyMedium?.copyWith(
                  color: theme.colorScheme.onSurfaceVariant,
                ),
                textAlign: TextAlign.center,
              ),
            ],
          ),
        ),
      ),
    );
  }

  Widget _buildFavoriteItem(BuildContext context, FavoriteStop fav) {
    final theme = Theme.of(context);
    final TransportType type = TransportType.fromValue(fav.routeType);
    final Color transportColor = colorFromHex(GTFSService.routeColors[type]!);
    final IconData transportIcon = GTFSService.getStopIcon(type);

    return Padding(
      padding: const EdgeInsets.only(bottom: 12),
      child: Dismissible(
        key: Key(fav.stopCode),
        direction: DismissDirection.endToStart,
        onDismissed: (_) async {
          await FavouritesRepository.instance.toggle(fav);
        },
        background: Container(
          alignment: Alignment.centerRight,
          padding: const EdgeInsets.only(right: 20),
          decoration: BoxDecoration(
            color: theme.colorScheme.errorContainer,
            borderRadius: BorderRadius.circular(16),
          ),
          child: Icon(
            Icons.delete_outline_rounded,
            color: theme.colorScheme.error,
          ),
        ),
        child: Card(
          elevation: 2,
          margin: EdgeInsets.zero,
          color: theme.colorScheme.surfaceContainerLow,
          shape: RoundedRectangleBorder(
            borderRadius: BorderRadius.circular(16),
            side: BorderSide(
              color: theme.colorScheme.outlineVariant.withOpacity(0.5),
            ),
          ),
          child: Material(
            color: Colors.transparent,
            child: InkWell(
              borderRadius: BorderRadius.circular(16),
              onTap: () => onFavouriteSelected(fav),
              child: Padding(
                padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 16),
                child: Row(
                  children: [
                    CircleAvatar(
                      backgroundColor: transportColor,
                      child: Icon(transportIcon, color: Colors.white, size: 20),
                    ),
                    const SizedBox(width: 16),
                    Expanded(
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Text(
                            fav.stopName,
                            style: theme.textTheme.titleMedium?.copyWith(
                              fontWeight: FontWeight.w600,
                            ),
                          ),
                          const SizedBox(height: 4),
                          Text(
                            "Код на спирка: ${fav.stopCode}",
                            style: theme.textTheme.bodyMedium?.copyWith(
                              color: theme.colorScheme.onSurfaceVariant,
                            ),
                          ),
                        ],
                      ),
                    ),
                    Icon(
                      Icons.arrow_forward_ios_rounded,
                      size: 20,
                      color: theme.colorScheme.outline,
                    ),
                  ],
                ),
              ),
            ),
          ),
        ),
      ),
    );
  }
}