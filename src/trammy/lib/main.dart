import 'package:flutter/material.dart';
import 'package:flutter_local_notifications/flutter_local_notifications.dart';
import 'package:shared_preferences/shared_preferences.dart';
import 'package:trammy/screens/database_loading_screen.dart';
import 'package:trammy/screens/main_screen.dart';
import 'package:trammy/services/gtfs_service.dart';
import 'package:workmanager/workmanager.dart';

void main() async {
  WidgetsFlutterBinding.ensureInitialized();

  Workmanager().initialize(callbackDispatcher);
  
  final now = DateTime.now();
  var firstRun = DateTime(now.year, now.month, now.day, 5);
  if (firstRun.isBefore(now) || firstRun.isAtSameMomentAs(now)) {
    firstRun = firstRun.add(const Duration(days: 1));
  }

  final initialDelay = firstRun.difference(now);

  var prefs = await SharedPreferences.getInstance();
  
  bool autoUpdate = prefs.getBool('settings_auto_update') ?? true;
  bool wifiOnly = prefs.getBool('settings_wifi_only') ?? true;

  if (autoUpdate) {
    await Workmanager().registerPeriodicTask(
      "gtfsUpdateTaskId",
      "gtfsUpdateTask",
      frequency: const Duration(hours: 1),
      //initialDelay: initialDelay,
      constraints: Constraints(
        networkType: wifiOnly ? NetworkType.unmetered : NetworkType.connected,
      ),
      existingWorkPolicy: ExistingPeriodicWorkPolicy.keep,
    );
  }

  bool dbLoaded = prefs.getBool('dbLoaded') ?? false;

  runApp(MyApp(dbLoaded: dbLoaded));

}

final FlutterLocalNotificationsPlugin localNotifications = FlutterLocalNotificationsPlugin();

@pragma('vm:entry-point')
void callbackDispatcher() {
  Workmanager().executeTask((task, inputData) async {
    print('[Workmanager] ✅ Dart isolate started, task: $task');

    if (task == "gtfsUpdateTask") {
      try {
        final prefs = await SharedPreferences.getInstance();

        final dbLoaded = prefs.getBool('dbLoaded') ?? false;
        if (!dbLoaded) {
          print('[Workmanager] DB not ready, skipping');
          return Future.value(true);
        }

        const androidInit = AndroidInitializationSettings('@mipmap/ic_launcher');
        await localNotifications.initialize(
          const InitializationSettings(android: androidInit),
        );

        final androidPlugin = localNotifications
            .resolvePlatformSpecificImplementation<AndroidFlutterLocalNotificationsPlugin>();

        await androidPlugin?.createNotificationChannel(
          const AndroidNotificationChannel(
            'gtfs_channel',
            'GTFS updates',
            description: 'Database update notifications',
            importance: Importance.defaultImportance,
          ),
        );

        await GTFSService.init();
        await GTFSService.updateGTFS(onProgress: (_) {});

        await prefs.setInt(
          'settings_last_updated',
          DateTime.now().millisecondsSinceEpoch,
        );

        final notificationsEnabled =
            prefs.getBool('settings_update_notifications') ?? true;

        if (notificationsEnabled) {
          await localNotifications.show(
            0,
            'Trammy',
            'Беше извършено обновяване на разписанията.',
            const NotificationDetails(
              android: AndroidNotificationDetails(
                'gtfs_channel',
                'GTFS updates',
                channelDescription: 'Database update notifications',
                importance: Importance.defaultImportance,
              ),
            ),
          );
        }

        print('[Workmanager] GTFS update completed');
        return Future.value(true);
      } catch (e, stack) {
        print('[Workmanager] GTFS update failed: $e');
        print(stack);
        return Future.value(false);
      }
    }

    return Future.value(true);
  });
}

class MyApp extends StatelessWidget {
  const MyApp({super.key, required this.dbLoaded});

  final bool dbLoaded;

  // This widget is the root of your application.
  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'Trammy',
      theme: ThemeData(
        // This is the theme of your application.
        //
        // TRY THIS: Try running your application with "flutter run". You'll see
        // the application has a purple toolbar. Then, without quitting the app,
        // try changing the seedColor in the colorScheme below to Colors.green
        // and then invoke "hot reload" (save your changes or press the "hot
        // reload" button in a Flutter-supported IDE, or press "r" if you used
        // the command line to start the app).
        //
        // Notice that the counter didn't reset back to zero; the application
        // state is not lost during the reload. To reset the state, use hot
        // restart instead.
        //
        // This works for code too, not just values: Most code changes can be
        // tested with just a hot reload.
        colorScheme: ColorScheme.fromSeed(seedColor: Colors.deepPurple),
      ),
      home: dbLoaded ? const MainScreen() : DatabaseLoadingScreen(),
      darkTheme: ThemeData.dark(),
    );
  }
}
