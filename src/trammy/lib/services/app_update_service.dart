import 'dart:io';
import 'package:dio/dio.dart';
import 'package:open_file/open_file.dart';
import 'package:package_info_plus/package_info_plus.dart';
import 'package:path_provider/path_provider.dart';
import 'package:permission_handler/permission_handler.dart';
import 'package:trammy/services/common.dart';

class AppUpdateService {
  static const githubApiUrl =
      'https://api.github.com/repos/angelinn/trammy/releases/latest';

  static Future<({String version, String downloadUrl})?> checkForUpdate() async {
    try {
      final info = await PackageInfo.fromPlatform();
      final currentVersion = info.version;

      final dio = Dio();
      final response = await dio.get(githubApiUrl);
      final tagName = response.data['tag_name'] as String;
      final latestVersion = tagName.replaceFirst('v', '');

      if (!isNewer(latestVersion, currentVersion)) return null;

      DebugLogger.append('[AppUpdateService] New version available: $latestVersion');
      
      final assets = response.data['assets'] as List;
      final apkAsset = assets.firstWhere(
        (a) => (a['name'] as String).endsWith('.apk'),
        orElse: () => null,
      );

      if (apkAsset == null) {
        DebugLogger.append('[AppUpdateService] No APK asset found in the latest release.');
        return null;
      }

      return (
        version: latestVersion,
        downloadUrl: apkAsset['browser_download_url'] as String,
      );
    } catch (e) {
      DebugLogger.append('[AppUpdateService] check failed: $e');
      return null;
    }
  }

  static Future<void> downloadAndInstall(
    String downloadUrl, {
    void Function(double progress)? onProgress,
  }) async {
    final permission = await Permission.requestInstallPackages.request();
    if (!permission.isGranted) {
      DebugLogger.append('[AppUpdateService] Install permission denied.');
      throw Exception('Install permission denied');
    }

    final dir = await getExternalStorageDirectory();
    final savePath = '${dir!.path}/trammy_update.apk';

    final dio = Dio();
    await dio.download(
      downloadUrl,
      savePath,
      onReceiveProgress: (received, total) {
        if (total > 0 && onProgress != null) {
          onProgress(received / total);
        }
      },
    );

    await OpenFile.open(savePath);
  }

  static bool isNewer(String latest, String current) {
    final l = latest.split('.').map(int.parse).toList();
    final c = current.split('.').map(int.parse).toList();

    for (int i = 0; i < l.length; i++) {
      if (l[i] > c[i]) return true;
      if (l[i] < c[i]) return false;
    }
    return false;
  }
}
