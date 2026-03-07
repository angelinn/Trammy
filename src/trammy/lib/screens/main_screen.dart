import 'package:flutter/material.dart';
import 'package:trammy/controllers/map_screen_controller.dart';
import 'package:trammy/models/favourite.dart';
import 'package:trammy/screens/favourites_screen.dart';
import 'package:trammy/screens/map/map_screen.dart';

class MainScreen extends StatefulWidget {
  const MainScreen({super.key});

  @override
  State<MainScreen> createState() => MainScreenState();
}

class MainScreenState extends State<MainScreen> {
  int selectedIndex = 0;
  final GlobalKey<MapScreenState> mapKey = GlobalKey<MapScreenState>(); 
  late List<Widget> screens;

  @override
  void initState() {
    super.initState();
    screens = [
      MapScreen(title: 'Map Screen', key: mapKey),
      FavouritesScreen(onFavouriteSelected: onFavouriteSelected)
    ];
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
          )
        ],
      )
    );
  }
}