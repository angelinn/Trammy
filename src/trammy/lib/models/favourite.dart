class FavoriteStop {
  final String stopCode;
  final String stopName;
  final int routeType; // Dominant vehicle type (0 for Tram, 3 for Bus, etc.)

  FavoriteStop({
    required this.stopCode,
    required this.stopName,
    required this.routeType,
  });

  // Convert a FavoriteStop into a Map. The keys must correspond to the column names in the DB.
  Map<String, dynamic> toMap() {
    return {
      'stop_code': stopCode,
      'stop_name': stopName,
      'route_type': routeType,
    };
  }

  // Extract a FavoriteStop object from a Map.
  factory FavoriteStop.fromMap(Map<String, dynamic> map) {
    return FavoriteStop(
      stopCode: map['stop_code'],
      stopName: map['stop_name'],
      routeType: map['route_type'],
    );
  }
}