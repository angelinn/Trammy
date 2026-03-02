class Level {
  final String levelId;
  final double? levelIndex;
  final String? levelName;

  Level({
    required this.levelId,
    this.levelIndex,
    this.levelName,
  });

  factory Level.fromMap(Map<String, dynamic> map) => Level(
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