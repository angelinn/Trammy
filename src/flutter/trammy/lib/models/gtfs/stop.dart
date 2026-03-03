class GTFSStop {
  final String stopId;
  final String? stopCode;
  final String? stopName;
  final String? stopDesc;
  final double? stopLat;
  final double? stopLon;
  final int? locationType;
  final String? parentStation;
  final String? stopTimezone;
  final String? levelId;

  GTFSStop({
    required this.stopId,
    this.stopCode,
    this.stopName,
    this.stopDesc,
    this.stopLat,
    this.stopLon,
    this.locationType,
    this.parentStation,
    this.stopTimezone,
    this.levelId,
  });

  factory GTFSStop.fromMap(Map<String, dynamic> map) => GTFSStop(
        stopId: map['stop_id'],
        stopCode: map['stop_code'],
        stopName: map['stop_name'],
        stopDesc: map['stop_desc'],
        stopLat: double.tryParse(map['stop_lat'].toString()),
        stopLon: double.tryParse(map['stop_lon'].toString()),
        locationType: (map['location_type'] as num?)?.toInt(),
        parentStation: map['parent_station'],
        stopTimezone: map['stop_timezone'],
        levelId: map['level_id'],
      );

  Map<String, dynamic> toMap() => {
        'stop_id': stopId,
        'stop_code': stopCode,
        'stop_name': stopName,
        'stop_desc': stopDesc,
        'stop_lat': stopLat,
        'stop_lon': stopLon,
        'location_type': locationType,
        'parent_station': parentStation,
        'stop_timezone': stopTimezone,
        'level_id': levelId,
      };
}

class GTFSStopRouteInfo {
  final String stopId;
  final String? stopCode;
  final String? stopName;
  final String? stopDesc;
  final double? stopLat;
  final double? stopLon;
  final int? locationType;
  final String? parentStation;
  final String? stopTimezone;
  final String? levelId;
  final int? routeType;
  final String? routeColor;

  GTFSStopRouteInfo({
    required this.stopId,
    this.stopCode,
    this.stopName,
    this.stopDesc,
    this.stopLat,
    this.stopLon,
    this.locationType,
    this.parentStation,
    this.stopTimezone,
    this.levelId,
    this.routeType,
    this.routeColor,
  });

  factory GTFSStopRouteInfo.fromMap(Map<String, dynamic> map) => GTFSStopRouteInfo(
        stopId: map['stop_id'],
        stopCode: map['stop_code'],
        stopName: map['stop_name'],
        stopDesc: map['stop_desc'],
        stopLat: map['stop_lat'] != null ? double.tryParse(map['stop_lat'].toString()) : null,
        stopLon: map['stop_lon'] != null ? double.tryParse(map['stop_lon'].toString()) : null,
        locationType: (map['location_type'] as num?)?.toInt(),
        parentStation: map['parent_station'],
        stopTimezone: map['stop_timezone'],
        levelId: map['level_id'],
        routeType: map['route_type'] is int
            ? map['route_type']
            : int.tryParse(map['route_type']?.toString() ?? ''),
        routeColor: map['route_color'],
      );

  Map<String, dynamic> toMap() => {
        'stop_id': stopId,
        'stop_code': stopCode,
        'stop_name': stopName,
        'stop_desc': stopDesc,
        'stop_lat': stopLat,
        'stop_lon': stopLon,
        'location_type': locationType,
        'parent_station': parentStation,
        'stop_timezone': stopTimezone,
        'level_id': levelId,
        'route_type': routeType,
        'route_color': routeColor,
      };
}