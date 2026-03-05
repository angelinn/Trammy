class GTFSLevel {
  final String levelId;
  final double? levelIndex;
  final String? levelName;

  GTFSLevel({
    required this.levelId,
    this.levelIndex,
    this.levelName,
  });

  factory GTFSLevel.fromMap(Map<String, dynamic> map) => GTFSLevel(
        levelId: map['level_id'],
        levelIndex: (map['level_index'] as num?)?.toDouble(),
        levelName: map['level_name'],
      );

  Map<String, dynamic> toMap() => {
        'level_id': levelId,
        'level_index': levelIndex,
        'level_name': levelName,
      };
}