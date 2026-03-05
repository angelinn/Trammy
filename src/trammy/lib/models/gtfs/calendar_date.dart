class GTFSCalendarDate {
  final String serviceId;
  final DateTime date;
  final int exceptionType; // 1 = added, 2 = removed

    GTFSCalendarDate({
    required this.serviceId,
    required this.date,
    required this.exceptionType,
  });

  /// Create from SQLite map
  factory GTFSCalendarDate.fromMap(Map<String, dynamic> map) {
    return GTFSCalendarDate(
      serviceId: map['service_id'].toString(),
      date: _parseGtfsDate(map['date']),
      exceptionType: _toInt(map['exception_type']),
    );
  }

  /// Convert to SQLite map
  Map<String, dynamic> toMap() {
    return {
      'service_id': serviceId,
      'date': _formatGtfsDate(date),
      'exception_type': exceptionType,
    };
  }

  /// Helper: parse YYYYMMDD (GTFS format)
  static DateTime _parseGtfsDate(dynamic value) {
    final str = value.toString();
    return DateTime(
      int.parse(str.substring(0, 4)),
      int.parse(str.substring(4, 6)),
      int.parse(str.substring(6, 8)),
    );
  }

  /// Helper: convert DateTime → YYYYMMDD
  static String _formatGtfsDate(DateTime date) {
    return '${date.year.toString().padLeft(4, '0')}'
        '${date.month.toString().padLeft(2, '0')}'
        '${date.day.toString().padLeft(2, '0')}';
  }

  static int _toInt(dynamic value) {
    if (value is int) return value;
    if (value is String) return int.tryParse(value) ?? 0;
    return 0;
  }

  bool get isAdded => exceptionType == 1;
  bool get isRemoved => exceptionType == 2;
}