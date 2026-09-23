import 'dart:io';
import 'dart:ui';
import 'dart:math';

int? parseInt(dynamic value) {
  if (value == null) return null;
  if (value is int) return value;
  if (value is String && value.trim().isNotEmpty) {
    return int.tryParse(value);
  }
  return null;
}

Color colorFromHex(String hexString) {
  final buffer = StringBuffer();
  if (hexString.length == 6 || hexString.length == 7)
    buffer.write('ff'); // add opacity if missing
  buffer.write(hexString.replaceFirst('#', ''));
  return Color(int.parse(buffer.toString(), radix: 16));
}

DateTime fromUnixTime(int unixTime) { 
  return DateTime.fromMillisecondsSinceEpoch(unixTime * 1000);
}

double bearingBetween(double lat1, double lon1, double lat2, double lon2) {
  double toRad(double d) => d * (pi / 180.0);
  double toDeg(double r) => r * (180.0 / pi);

  final dLon = toRad(lon2 - lon1);
  final aLat = toRad(lat1);
  final bLat = toRad(lat2);

  final y = sin(dLon) * cos(bLat);
  final x = cos(aLat) * sin(bLat) - sin(aLat) * cos(bLat) * cos(dLon);

  final brg = (toDeg(atan2(y, x)) + 360) % 360;

  return brg;
}


class DebugLogger {
  static Future<void> append(String message) async {
    try {
      Directory generalDownloadDir = Directory('/storage/emulated/0/Download');
      final file = File('${generalDownloadDir.path}/trammy_debug.txt');
      final sink = file.openWrite(mode: FileMode.append);
      sink.writeln('${DateTime.now().toIso8601String()} $message');
      await sink.close();

      print('${DateTime.now().toIso8601String()} $message');
    } catch (_) {
      print('Failed to write debug log: $message');
    }
  }
}