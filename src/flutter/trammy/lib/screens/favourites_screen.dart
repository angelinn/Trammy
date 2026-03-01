
import 'package:flutter/material.dart';

class FavouritesScreen extends StatefulWidget {
  const FavouritesScreen({super.key, required this.title});

  final String title;

  @override
  State<FavouritesScreen> createState() => FavouritesScreenState();
}

class FavouritesScreenState extends State<FavouritesScreen> {
  List<String> favourites = [
    "Ул. Буря",
    "Ул. Стефан Пешев",
    "Кв. Карпузица"
    "Площад Сред Село"
  ];


  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Favourites'),
      ),
      body: ListView.builder(
        itemCount: favourites.length,
        itemBuilder: (context, index) {
          return ListTile(
            title: Text(favourites[index]),
          );
        },
      ),
    );
  }
}