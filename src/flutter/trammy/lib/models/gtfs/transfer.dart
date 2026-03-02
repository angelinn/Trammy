class Shape {
  final String shapeId;
  final double shapePtLat;
  final double shapePtLon;
  final int shapePtSequence;
  final double? shapeDistTraveled;

  Shape({
    required this.shapeId,
    required this.shapePtLat,
    required this.shapePtLon,
    required this.shapePtSequence,
    this.shapeDistTraveled,
  });

  factory Shape.fromMap(Map<String, dynamic> map) => Shape(
        shapeId: map['shape_id'],
        shapePtLat: (map['shape_pt_lat'] as num).toDouble(),
        shapePtLon: (map['shape_pt_lon'] as num).toDouble(),
        shapePtSequence: map['shape_pt_sequence'],
        shapeDistTraveled:
            (map['shape_dist_traveled'] as num?)?.toDouble(),
      );

  Map<String, dynamic> toMap() => {
        'shape_id': shapeId,
        'shape_pt_lat': shapePtLat,
        'shape_pt_lon': shapePtLon,
        'shape_pt_sequence': shapePtSequence,
        'shape_dist_traveled': shapeDistTraveled,
      };
}