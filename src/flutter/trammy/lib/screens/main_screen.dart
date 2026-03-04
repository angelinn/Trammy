import 'package:flutter/material.dart';
import 'package:trammy/screens/favourites_screen.dart';
import 'package:trammy/screens/map/map_screen.dart';

class MainScreen extends StatefulWidget {
  const MainScreen({super.key});

  @override
  State<MainScreen> createState() => MainScreenState();
}

class MainScreenState extends State<MainScreen> {
  int selectedIndex = 0;

  final List<Widget> screens = [
    MapScreen(title: 'Map Screen'),
    FavouritesScreen(title: 'Favourites Screen')
  ];

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