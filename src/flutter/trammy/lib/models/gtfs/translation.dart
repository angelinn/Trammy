class Translation {
  final String? tableName;
  final String? fieldName;
  final String? language;
  final String? translation;
  final String? recordId;
  final String? recordSubId;
  final String? fieldValue;

  Translation({
    this.tableName,
    this.fieldName,
    this.language,
    this.translation,
    this.recordId,
    this.recordSubId,
    this.fieldValue,
  });

  factory Translation.fromMap(Map<String, dynamic> map) => Translation(
        tableName: map['table_name'],
        fieldName: map['field_name'],
        language: map['language'],
        translation: map['translation'],
        recordId: map['record_id'],
        recordSubId: map['record_sub_id'],
        fieldValue: map['field_value'],
      );

  Map<String, dynamic> toMap() => {
        'table_name': tableName,
        'field_name': fieldName,
        'language': language,
        'translation': translation,
        'record_id': recordId,
        'record_sub_id': recordSubId,
        'field_value': fieldValue,
      };
}