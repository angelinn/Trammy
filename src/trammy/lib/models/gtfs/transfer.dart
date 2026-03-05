class GTFSTransfer {
  final String fromStopId;
  final String toStopId;
  final String? fromRouteId;
  final String? toRouteId;
  final String? fromTripId;
  final String? toTripId;
  final int? transferType;
  final int? minTransferTime; // seconds

  GTFSTransfer({
    required this.fromStopId,
    required this.toStopId,
    this.fromRouteId,
    this.toRouteId,
    this.fromTripId,
    this.toTripId,
    this.transferType,
    this.minTransferTime,
  });

  factory GTFSTransfer.fromMap(Map<String, dynamic> map) {
    return GTFSTransfer(
      fromStopId: map['from_stop_id'].toString(),
      toStopId: map['to_stop_id'].toString(),
      fromRouteId: map['from_route_id']?.toString(),
      toRouteId: map['to_route_id']?.toString(),
      fromTripId: map['from_trip_id']?.toString(),
      toTripId: map['to_trip_id']?.toString(),
      transferType: _toIntNullable(map['transfer_type']),
      minTransferTime: _toIntNullable(map['min_transfer_time']),
    );
  }

  Map<String, dynamic> toMap() {
    return {
      'from_stop_id': fromStopId,
      'to_stop_id': toStopId,
      'from_route_id': fromRouteId,
      'to_route_id': toRouteId,
      'from_trip_id': fromTripId,
      'to_trip_id': toTripId,
      'transfer_type': transferType,
      'min_transfer_time': minTransferTime,
    };
  }

  static int? _toIntNullable(dynamic value) {
    if (value == null) return null;
    if (value is int) return value;
    if (value is String) return int.tryParse(value);
    return null;
  }

  // GTFS transfer types
  bool get isRecommended => transferType == 0;
  bool get isTimed => transferType == 1;
  bool get requiresMinTime => transferType == 2;
  bool get noTransfer => transferType == 3;
}