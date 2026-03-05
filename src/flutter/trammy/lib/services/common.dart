import 'dart:ui';

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
