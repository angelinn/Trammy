class GTFSFeedInfo {
  final String? feedPublisherName;
  final String? feedPublisherUrl;
  final String? feedLang;
  final String? defaultLang;
  final String? feedStartDate;
  final String? feedEndDate;
  final String? feedVersion;
  final String? feedContactEmail;
  final String? feedContactUrl;

  GTFSFeedInfo({
    this.feedPublisherName,
    this.feedPublisherUrl,
    this.feedLang,
    this.defaultLang,
    this.feedStartDate,
    this.feedEndDate,
    this.feedVersion,
    this.feedContactEmail,
    this.feedContactUrl,
  });

  factory GTFSFeedInfo.fromMap(Map<String, dynamic> map) => GTFSFeedInfo(
        feedPublisherName: map['feed_publisher_name'],
        feedPublisherUrl: map['feed_publisher_url'],
        feedLang: map['feed_lang'],
        defaultLang: map['default_lang'],
        feedStartDate: map['feed_start_date'],
        feedEndDate: map['feed_end_date'],
        feedVersion: map['feed_version'],
        feedContactEmail: map['feed_contact_email'],
        feedContactUrl: map['feed_contact_url'],
      );

  Map<String, dynamic> toMap() => {
        'feed_publisher_name': feedPublisherName,
        'feed_publisher_url': feedPublisherUrl,
        'feed_lang': feedLang,
        'default_lang': defaultLang,
        'feed_start_date': feedStartDate,
        'feed_end_date': feedEndDate,
        'feed_version': feedVersion,
        'feed_contact_email': feedContactEmail,
        'feed_contact_url': feedContactUrl,
      };
}