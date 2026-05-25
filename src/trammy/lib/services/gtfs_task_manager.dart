import 'package:flutter_local_notifications/flutter_local_notifications.dart';
import 'package:shared_preferences/shared_preferences.dart';
import 'package:trammy/services/gtfs_service.dart';
import 'package:workmanager/workmanager.dart';

final FlutterLocalNotificationsPlugin localNotifications =
    FlutterLocalNotificationsPlugin();

class GTFSTaskManager {
  static const taskId = 'gtfsUpdateTaskId';
  static const taskName = 'gtfsUpdateTask';

  static Future<void> registerOnStartup() async {
    final prefs = await SharedPreferences.getInstance();
    final autoUpdate = prefs.getBool('settings_auto_update') ?? true;
    final wifiOnly = prefs.getBool('settings_wifi_only') ?? true;

    if (!autoUpdate) return;

    await Workmanager().registerPeriodicTask(
      taskId,
      taskName,
      frequency: const Duration(hours: 1),
      //initialDelay: nextFiveAm(),
      constraints: Constraints(
        networkType: wifiOnly ? NetworkType.unmetered : NetworkType.connected,
      ),
      existingWorkPolicy: ExistingPeriodicWorkPolicy.keep,
    );
  }

  static Future<void> reschedule({
    required bool autoUpdate,
    required bool wifiOnly,
  }) async {
    await Workmanager().cancelByUniqueName(taskId);
    if (!autoUpdate) return;

    await Workmanager().registerPeriodicTask(
      taskId,
      taskName,
      frequency: const Duration(hours: 1),
      //initialDelay: nextFiveAm(),
      constraints: Constraints(
        networkType: wifiOnly ? NetworkType.unmetered : NetworkType.connected,
      ),
      existingWorkPolicy: ExistingPeriodicWorkPolicy.update,
    );
  }

  static Future<void> scheduleOneOff({
    required int minutes,
    required bool wifiOnly,
  }) async {
    await Workmanager().registerOneOffTask(
      'gtfsManualUpdate_${DateTime.now().millisecondsSinceEpoch}',
      taskName,
      initialDelay: Duration(minutes: minutes),
      constraints: Constraints(
        networkType: wifiOnly ? NetworkType.unmetered : NetworkType.connected,
      ),
    );
  }

  static Future<bool> executeTask(String task) async {
    if (task != taskName) return true;

    final prefs = await SharedPreferences.getInstance();

    final dbLoaded = prefs.getBool('dbLoaded') ?? false;
    if (!dbLoaded) {
      print('[GtfsTaskManager] DB not ready, skipping');
      return true;
    }

    await initNotifications();
    await GTFSService.init();
    await GTFSService.updateGTFS(onProgress: (_) {});

    await prefs.setInt(
      'settings_last_updated',
      DateTime.now().millisecondsSinceEpoch,
    );

    final notificationsEnabled =
        prefs.getBool('settings_update_notifications') ?? true;

    if (notificationsEnabled) {
      await showUpdateNotification();
    }

    print('[GtfsTaskManager] GTFS update completed');
    return true;
  }

  static Duration nextFiveAm() {
    final now = DateTime.now();
    var firstRun = DateTime(now.year, now.month, now.day, 5);
    if (!firstRun.isAfter(now)) {
      firstRun = firstRun.add(const Duration(days: 1));
    }
    return firstRun.difference(now);
  }

  static Future<void> initNotifications() async {
    const androidInit = AndroidInitializationSettings('@mipmap/ic_launcher');
    await localNotifications.initialize(
      const InitializationSettings(android: androidInit),
    );

    final androidPlugin = localNotifications
        .resolvePlatformSpecificImplementation<
            AndroidFlutterLocalNotificationsPlugin>();

    await androidPlugin?.createNotificationChannel(
      const AndroidNotificationChannel(
        'gtfs_channel',
        'GTFS updates',
        description: 'Database update notifications',
        importance: Importance.defaultImportance,
      ),
    );
  }

  static Future<void> showUpdateNotification() async {
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
}