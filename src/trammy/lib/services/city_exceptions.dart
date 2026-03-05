
import 'package:trammy/models/gtfs/route.dart';
import 'package:trammy/models/gtfs/stop.dart';
import 'package:trammy/services/gtfs_service.dart';

class SofiaExceptions {
  static final REAL_TROLLEYS_SOFIA = ['1', '2', '3', '4', '5', '6', '7', '8', '9', '11'];

  static TransportType? getRealType(GTFSRoute route) {
     final type = TransportType.fromValue(route.routeType!);
    if (route.routeType == TransportType.trolley.value) {
      return REAL_TROLLEYS_SOFIA.contains(route.routeShortName) ? TransportType.trolley : TransportType.bus;
    }

    return type;
  }
}