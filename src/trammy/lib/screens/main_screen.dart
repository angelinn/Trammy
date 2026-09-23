import 'package:flutter/material.dart';
import 'package:trammy/db/favourites_repository.dart';
import 'package:trammy/models/favourite.dart';
import 'package:trammy/screens/favourites_screen.dart';
import 'package:trammy/screens/map/map_screen.dart';
import 'package:trammy/screens/settings_screen.dart';
import 'package:trammy/services/app_update_service.dart';

class MainScreen extends StatefulWidget {
  const MainScreen({super.key});

  @override
  State<MainScreen> createState() => MainScreenState();
}

class MainScreenState extends State<MainScreen> {
  int selectedIndex = 0;
  final GlobalKey<MapScreenState> mapKey = GlobalKey<MapScreenState>();   
  final GlobalKey<SettingsScreenState> settingsKey = GlobalKey<SettingsScreenState>(); 

  late List<Widget> screens;

  @override
  void initState() {
    super.initState();
    screens = [
      MapScreen(title: 'Map Screen', key: mapKey),
      FavouritesScreen(onFavouriteSelected: onFavouriteSelected),
      SettingsScreen(key: settingsKey),
    ];

    FavouritesRepository.instance.load();
    checkForUpdate();
  }

  Future<void> checkForUpdate() async {
    final update = await AppUpdateService.checkForUpdate();
    if (update == null || !mounted) return;

    showDialog(
      context: context,
      builder: (ctx) => AlertDialog(
        title: const Text('Налична актуализация'),
        content: Text('Версия ${update.version} е налична. Искате ли да я изтеглите?'),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(ctx),
            child: const Text('По-късно'),
          ),
          FilledButton.icon(
            onPressed: () {
              Navigator.pop(ctx);
              AppUpdateService.downloadAndInstall(update.downloadUrl);
            },
            icon: const Icon(Icons.download_rounded),
            label: const Text('Изтегли'),
          ),
        ],
      ),
    );
  }
  
  void onFavouriteSelected(FavoriteStop fav) async {
    setState(() {
      selectedIndex = 0;
    });

    Future.delayed(const Duration(milliseconds: 100), () {
       WidgetsBinding.instance.addPostFrameCallback((_) {
        mapKey.currentState?.onFavouriteTapped(fav.stopCode);
       });
    });
  }

  void onTabTapped(int index) {
    if (index == 2) {
      settingsKey.currentState?.loadSettings();
    }
    
    setState(() {
      selectedIndex = index;
    });
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: IndexedStack(
        index: selectedIndex,
        children: screens,
      ),
      bottomNavigationBar: NavigationBar(
        selectedIndex: selectedIndex,
        onDestinationSelected: onTabTapped,
        destinations: const [
          NavigationDestination(
            icon: Icon(Icons.map),
            label: 'Карта',
          ),
          NavigationDestination(
            icon: Icon(Icons.favorite),
            label: 'Любими',
          ),
          NavigationDestination(
            icon: Icon(Icons.settings_outlined),
            selectedIcon: Icon(Icons.settings),
            label: 'Настройки',
          ),
        ],
      )
    );
  }
}