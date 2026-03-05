import 'package:trammy/models/gtfs/stop.dart';

class GTFSRoute {
  final String routeId;
  final String? agencyId;
  final String? routeShortName;
  final String? routeLongName;
  final String? routeDesc;
  final int? routeType;
  final String? routeUrl;
  final String? routeColor;
  final String? routeTextColor;
  final int? routeSortOrder;
  final int? continuousPickup;
  final int? continuousDropOff;

  GTFSRoute({
    required this.routeId,
    this.agencyId,
    this.routeShortName,
    this.routeLongName,
    this.routeDesc,
    this.routeType,
    this.routeUrl,
    this.routeColor,
    this.routeTextColor,
    this.routeSortOrder,
    this.continuousPickup,
    this.continuousDropOff,
  });

  factory GTFSRoute.fromMap(Map<String, dynamic> map) => GTFSRoute(
        routeId: map['route_id'],
        agencyId: map['agency_id'],
        routeShortName: map['route_short_name'],
        routeLongName: map['route_long_name'],
        routeDesc: map['route_desc'],
        routeType: map['route_type'],
        routeUrl: map['route_url'],
        routeColor: map['route_color'],
        routeTextColor: map['route_text_color'],
        routeSortOrder: map['route_sort_order'],
        continuousPickup: map['continuous_pickup'],
        continuousDropOff: map['continuous_drop_off'],
      );

  Map<String, dynamic> toMap() => {
        'route_id': routeId,
        'agency_id': agencyId,
        'route_short_name': routeShortName,
        'route_long_name': routeLongName,
        'route_desc': routeDesc,
        'route_type': routeType,
        'route_url': routeUrl,
        'route_color': routeColor,
        'route_text_color': routeTextColor,
        'route_sort_order': routeSortOrder,
        'continuous_pickup': continuousPickup,
        'continuous_drop_off': continuousDropOff,
      };
}