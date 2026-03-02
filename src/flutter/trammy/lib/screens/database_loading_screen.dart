import 'package:flutter/material.dart';
import 'package:shared_preferences/shared_preferences.dart';
import 'package:trammy/screens/main_screen.dart';
import 'package:trammy/services/gtfs_service.dart';

class DatabaseLoadingScreen extends StatefulWidget {
  const DatabaseLoadingScreen({super.key});

  @override
  State<DatabaseLoadingScreen> createState() => DatabaseLoadingScreenState();
}

class DatabaseLoadingScreenState extends State<DatabaseLoadingScreen> {
  GTFSProgress? progress;

  @override
  void initState() {
    super.initState();
    downloadGTFSData();
  }

  Future<void> downloadGTFSData() async {
    try {
      await GTFSService.updateGTFS(
        onProgress: (p) {
          setState(() {
            progress = p;
          });
        },
      );

      setState(() {});
    } catch (e) {
      debugPrint('Error loading GTFS: $e');
    }

    var prefs = await SharedPreferences.getInstance();
    await prefs.setBool('dbLoaded', true); 

    if (!mounted) return;

    Navigator.of(context).pushReplacement(
    MaterialPageRoute(
        builder: (_) => const MainScreen(),
    ),
);
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Подготовка на данни')),

      body: Center(
        child: Padding(
          padding: const EdgeInsets.all(24),
          child: Column(
            mainAxisAlignment: MainAxisAlignment.center,
            children: [
              if (progress != null) ...[
                Text(
                  'Обработка на: ${progress!.table}',
                  style: const TextStyle(fontSize: 18),
                ),
                const SizedBox(height: 12),
                const CircularProgressIndicator(),
                const SizedBox(height: 12),
                Text('Въвеждане на ${progress!.current} реда...'),
              ] else ...[
                const CircularProgressIndicator(),
                const SizedBox(height: 16),
                const Text('Сваляне на GTFS данни...'),
              ],
            ],
          ),
        ),
      ),
    );
  }
}
