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
  final String? routeTypes;
  final String? routeColors;
  final String? routeIds;

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
    this.routeTypes,
    this.routeColors,
    this.routeIds,
  }); 

  factory GTFSStopRouteInfo.fromMap(Map<String, dynamic> map) =>
      GTFSStopRouteInfo(
        stopId: map['stop_id'],
        stopCode: map['stop_code'],
        stopName: map['stop_name'],
        stopDesc: map['stop_desc'],
        stopLat: map['stop_lat'] != null
            ? double.tryParse(map['stop_lat'].toString())
            : null,
        stopLon: map['stop_lon'] != null
            ? double.tryParse(map['stop_lon'].toString())
            : null,
        locationType: (map['location_type'] as num?)?.toInt(),
        parentStation: map['parent_station'],
        stopTimezone: map['stop_timezone'],
        levelId: map['level_id'],
        routeTypes: map['route_types'],
        routeColors: map['route_colors'],
        routeIds: map['route_ids'],
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
    'route_types': routeTypes,
    'route_colors': routeColors,
    'route_ids': routeIds,
  };
}

enum TransportType {
  tram(0),
  subway(1),
  bus(3),
  night(-1),
  trolley(11);

  const TransportType(this.value);
  factory TransportType.fromValue(int value) => TransportType.values.firstWhere((e) => e.value == value);
  final int value;
}
