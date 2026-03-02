class GTFSPathway {
  final String pathwayId;
  final String? fromStopId;
  final String? toStopId;
  final int? pathwayMode;
  final int? isBidirectional;
  final double? length;
  final int? traversalTime;
  final int? stairCount;
  final double? maxSlope;
  final double? minWidth;
  final String? signpostedAs;
  final String? reversedSignpostedAs;

  GTFSPathway({
    required this.pathwayId,
    this.fromStopId,
    this.toStopId,
    this.pathwayMode,
    this.isBidirectional,
    this.length,
    this.traversalTime,
    this.stairCount,
    this.maxSlope,
    this.minWidth,
    this.signpostedAs,
    this.reversedSignpostedAs,
  });

  factory GTFSPathway.fromMap(Map<String, dynamic> map) => GTFSPathway(
        pathwayId: map['pathway_id'],
        fromStopId: map['from_stop_id'],
        toStopId: map['to_stop_id'],
        pathwayMode: map['pathway_mode'],
        isBidirectional: map['is_bidirectional'],
        length: (map['length'] as num?)?.toDouble(),
        traversalTime: map['traversal_time'],
        stairCount: map['stair_count'],
        maxSlope: (map['max_slope'] as num?)?.toDouble(),
        minWidth: (map['min_width'] as num?)?.toDouble(),
        signpostedAs: map['signposted_as'],
        reversedSignpostedAs: map['reversed_signposted_as'],
      );

  Map<String, dynamic> toMap() => {
        'pathway_id': pathwayId,
        'from_stop_id': fromStopId,
        'to_stop_id': toStopId,
        'pathway_mode': pathwayMode,
        'is_bidirectional': isBidirectional,
        'length': length,
        'traversal_time': traversalTime,
        'stair_count': stairCount,
        'max_slope': maxSlope,
        'min_width': minWidth,
        'signposted_as': signpostedAs,
        'reversed_signposted_as': reversedSignpostedAs,
      };
}