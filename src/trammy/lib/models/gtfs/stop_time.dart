class GTFSStopTime {
  final String tripId;
  final String? arrivalTime;
  final String? departureTime;
  final String stopId;
  final int stopSequence;
  final String? stopHeadsign;
  final int? pickupType;
  final int? dropOffType;
  final double? shapeDistTraveled;
  final int? continuousPickup;
  final int? continuousDropOff;
  final int? timepoint;

  GTFSStopTime({
    required this.tripId,
    required this.stopId,
    required this.stopSequence,
    this.arrivalTime,
    this.departureTime,
    this.stopHeadsign,
    this.pickupType,
    this.dropOffType,
    this.shapeDistTraveled,
    this.continuousPickup,
    this.continuousDropOff,
    this.timepoint,
  });

  factory GTFSStopTime.fromMap(Map<String, dynamic> map) => GTFSStopTime(
        tripId: map['trip_id'],
        arrivalTime: map['arrival_time'],
        departureTime: map['departure_time'],
        stopId: map['stop_id'],
        stopSequence: map['stop_sequence'],
        stopHeadsign: map['stop_headsign'],
        pickupType: map['pickup_type'],
        dropOffType: map['drop_off_type'],
        shapeDistTraveled: (map['shape_dist_traveled'] as num?)?.toDouble(),
        continuousPickup: map['continuous_pickup'],
        continuousDropOff: map['continuous_drop_off'],
        timepoint: map['timepoint'],
      );

  Map<String, dynamic> toMap() => {
        'trip_id': tripId,
        'arrival_time': arrivalTime,
        'departure_time': departureTime,
        'stop_id': stopId,
        'stop_sequence': stopSequence,
        'stop_headsign': stopHeadsign,
        'pickup_type': pickupType,
        'drop_off_type': dropOffType,
        'shape_dist_traveled': shapeDistTraveled,
        'continuous_pickup': continuousPickup,
        'continuous_drop_off': continuousDropOff,
        'timepoint': timepoint,
      };
}

class GTFSStopTimeData {
  final String tripId;
  final String stopId;
  final String arrivalTime;     // raw GTFS format (HH:MM:SS, can be >24h)
  final String departureTime;   // raw GTFS format
  final int stopSequence;

  // Optional joined data
  final String? routeId;
  final String? tripHeadsign;
  final String? routeShortName;
  final int? routeType;
  final String? routeColor;

  GTFSStopTimeData({
    required this.tripId,
    required this.stopId,
    required this.arrivalTime,
    required this.departureTime,
    required this.stopSequence,
    this.routeId,
    this.tripHeadsign,
    this.routeShortName,
    this.routeType,
    this.routeColor,
  });

  /// Convert database row → object
  factory GTFSStopTimeData.fromMap(Map<String, dynamic> map) {
    return GTFSStopTimeData(
      tripId: map['trip_id'] as String,
      stopId: map['stop_id'] as String,
      arrivalTime: map['arrival_time'] as String,
      departureTime: map['departure_time'] as String,
      stopSequence: map['stop_sequence'] is int
          ? map['stop_sequence']
          : int.parse(map['stop_sequence'].toString()),

      routeId: map['route_id'] as String?,
      tripHeadsign: map['trip_headsign'] as String?,
      routeShortName: map['route_short_name'] as String?,
      routeType: map['route_type'] is int
          ? map['route_type']
          : map['route_type'] != null
              ? int.parse(map['route_type'].toString())
              : null,
      routeColor: map['route_color'] as String?,
    );
  }

  /// Convert object → database map
  Map<String, dynamic> toMap() {
    return {
      'trip_id': tripId,
      'stop_id': stopId,
      'arrival_time': arrivalTime,
      'departure_time': departureTime,
      'stop_sequence': stopSequence,
      'route_id': routeId,
      'trip_headsign': tripHeadsign,
      'route_short_name': routeShortName,
      'route_type': routeType,
      'route_color': routeColor,
    };
  }

  /// Convert GTFS time (HH:MM:SS, supports >24h) to DateTime
DateTime toArrivalDateTime() {
  final parts = arrivalTime.split(':');
  final hour = int.parse(parts[0]);
  final minute = int.parse(parts[1]);
  final second = int.parse(parts[2]);

  final now = DateTime.now();

  return DateTime(
    now.year,
    now.month,
    now.day,
    hour,
    minute,
    second,
  );
}

  DateTime toDepartureDateTime(DateTime serviceDate) {
    final parts = departureTime.split(':');
    final hour = int.parse(parts[0]);
    final minute = int.parse(parts[1]);
    final second = int.parse(parts[2]);

    return serviceDate
        .add(Duration(hours: hour, minutes: minute, seconds: second));
  }
}