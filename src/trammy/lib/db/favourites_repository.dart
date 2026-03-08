import 'package:flutter/material.dart';
import 'package:trammy/db/user_db_service.dart';
import 'package:trammy/models/favourite.dart';

class FavouritesRepository {
    FavouritesRepository._();
    static final instance = FavouritesRepository._();

    final ValueNotifier<List<FavoriteStop>> favorites = ValueNotifier([]);
    
    Future<void> load() async {
      favorites.value = await UserDbService.getFavorites();
    }

    Future<void> toggle(FavoriteStop fav) async {
      await UserDbService.toggleFavorite(fav);
      await load();
    }

    bool isFavorite(String stopCode) {
      return favorites.value.any((f) => f.stopCode == stopCode);
    }
}
