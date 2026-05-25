import 'package:flutter/material.dart';
import 'package:shared_preferences/shared_preferences.dart';
import 'package:trammy/services/gtfs_service.dart';
import 'package:trammy/screens/main_screen.dart';

class DatabaseLoadingScreen extends StatefulWidget {
  final bool force;
  const DatabaseLoadingScreen({super.key, this.force = false});

  @override
  State<DatabaseLoadingScreen> createState() => _DatabaseLoadingScreenState();
}

class _DatabaseLoadingScreenState extends State<DatabaseLoadingScreen> {
  String _currentAction = "Зареждане...";
  String _detailText = "";
  double? downloadProgress; // null = indeterminate


  @override
  void initState() {
    super.initState();
    _startSetup();
  }

  Future<void> _startSetup() async {
    try {
      await GTFSService.init();
      await GTFSService.updateGTFS(
        force: widget.force,
        onProgress: (progress) {
          if (!mounted) return;
          setState(() {
            _currentAction = progress.table;
            if (progress.table == 'Изтегляне') {
              final mb = progress.current / (1024 * 1024);
              _detailText = "${mb.toStringAsFixed(1)} MB / 43 MB";
              downloadProgress = progress.current / (43 * 1024 * 1024);
            } else if (progress.table == 'Разархивиране') {
              _detailText = "Подготвяне на разписанията...";
              downloadProgress = null; // back to indeterminate
            } else {
              _detailText = "";
              downloadProgress = null;
            }
          });
        },
      );

      final prefs = await SharedPreferences.getInstance();
      await prefs.setBool('dbLoaded', true);
      await prefs.setInt('settings_last_updated', DateTime.now().millisecondsSinceEpoch);

      if (mounted) {
        Navigator.of(context).pushReplacement(
          MaterialPageRoute(builder: (_) => const MainScreen()),
        );
      }
    } catch (e) {
      print(e);
      if (mounted) {
        setState(() {
          _currentAction = "Неуспешно сваляне";
          _detailText = "Опитайте отново.";
        });
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    
    return Scaffold(
      body: SafeArea(
        child: Padding(
          padding: const EdgeInsets.symmetric(horizontal: 40),
          child: Column(
            mainAxisAlignment: MainAxisAlignment.center,
            children: [
              const Spacer(),
              Icon(
                Icons.departure_board_rounded,
                size: 80,
                color: theme.colorScheme.primary,
              ),
              const SizedBox(height: 24),
              Text(
                "Изтегляне на разписания",
                style: theme.textTheme.headlineSmall?.copyWith(fontWeight: FontWeight.bold),
              ),
              const SizedBox(height: 12),
              Text(
                "Trammy сваля най-новите разписания.",
                textAlign: TextAlign.center,
                style: theme.textTheme.bodyMedium?.copyWith(color: theme.colorScheme.onSurfaceVariant),
              ),
              const SizedBox(height: 48),
              LinearProgressIndicator(
                value: downloadProgress,
                borderRadius: BorderRadius.all(Radius.circular(10)),
              ),
              const SizedBox(height: 16),
              Text(_currentAction, style: const TextStyle(fontWeight: FontWeight.w600)),
              if (_detailText.isNotEmpty)
                Text(_detailText, style: theme.textTheme.bodySmall?.copyWith(color: theme.colorScheme.outline)),
              const Spacer(),
              const Text("Trammy • GTFS обработка", style: TextStyle(fontSize: 10, letterSpacing: 1.2)),
              const SizedBox(height: 20),
            ],
          ),
        ),
      ),
    );
  }
}
