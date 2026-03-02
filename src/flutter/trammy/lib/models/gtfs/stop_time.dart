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