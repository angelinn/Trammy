class GTFSAgency {
  final String agencyId;
  final String? agencyName;
  final String? agencyUrl;
  final String? agencyTimezone;
  final String? agencyLang;
  final String? agencyPhone;
  final String? agencyEmail;

  GTFSAgency({
    required this.agencyId,
    this.agencyName,
    this.agencyUrl,
    this.agencyTimezone,
    this.agencyLang,
    this.agencyPhone,
    this.agencyEmail,
  });

  factory GTFSAgency.fromMap(Map<String, dynamic> map) => GTFSAgency(
        agencyId: map['agency_id'],
        agencyName: map['agency_name'],
        agencyUrl: map['agency_url'],
        agencyTimezone: map['agency_timezone'],
        agencyLang: map['agency_lang'],
        agencyPhone: map['agency_phone'],
        agencyEmail: map['agency_email'],
      );

  Map<String, dynamic> toMap() => {
        'agency_id': agencyId,
        'agency_name': agencyName,
        'agency_url': agencyUrl,
        'agency_timezone': agencyTimezone,
        'agency_lang': agencyLang,
        'agency_phone': agencyPhone,
        'agency_email': agencyEmail,
      };
}