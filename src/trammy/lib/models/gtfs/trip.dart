class GTFSTrip {
  final String tripId;
  final String routeId;
  final String serviceId;
  final String? headsign;
  final String? shortName;
  final int? directionId; // 0 or 1
  final String? blockId;
  final String? shapeId;
  final int? wheelchairAccessible;
  final int? bikesAllowed;

  GTFSTrip({
    required this.tripId,
    required this.routeId,
    required this.serviceId,
    this.headsign,
    this.shortName,
    this.directionId,
    this.blockId,
    this.shapeId,
    this.wheelchairAccessible,
    this.bikesAllowed,
  });

  factory GTFSTrip.fromMap(Map<String, dynamic> map) {
    return GTFSTrip(
      tripId: map['trip_id'].toString(),
      routeId: map['route_id'].toString(),
      serviceId: map['service_id'].toString(),
      headsign: map['trip_headsign']?.toString(),
      shortName: map['trip_short_name']?.toString(),
      directionId: _toIntNullable(map['direction_id']),
      blockId: map['block_id']?.toString(),
      shapeId: map['shape_id']?.toString(),
      wheelchairAccessible: _toIntNullable(map['wheelchair_accessible']),
      bikesAllowed: _toIntNullable(map['bikes_allowed']),
    );
  }

  Map<String, dynamic> toMap() {
    return {
      'trip_id': tripId,
      'route_id': routeId,
      'service_id': serviceId,
      'trip_headsign': headsign,
      'trip_short_name': shortName,
      'direction_id': directionId,
      'block_id': blockId,
      'shape_id': shapeId,
      'wheelchair_accessible': wheelchairAccessible,
      'bikes_allowed': bikesAllowed,
    };
  }

  static int? _toIntNullable(dynamic value) {
    if (value == null) return null;
    if (value is int) return value;
    if (value is String) return int.tryParse(value);
    return null;
  }

  // Helpful derived properties
  bool get isDirection0 => directionId == 0;
  bool get isDirection1 => directionId == 1;

  bool get isWheelchairAccessible => wheelchairAccessible == 1;
  bool get allowsBikes => bikesAllowed == 1;
}