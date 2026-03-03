int? parseInt(dynamic value) {
  if (value == null) return null;
  if (value is int) return value;
  if (value is String && value.trim().isNotEmpty) {
    return int.tryParse(value);
  }
  return null;
}
