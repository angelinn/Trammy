// stops_screen.dart

import 'dart:convert';
import 'package:flutter/material.dart';
import 'package:http/http.dart' as http;
import 'package:archive/archive.dart';
import 'package:csv/csv.dart';

/// Model for a GTFS Stop
class Stop {
  final String id;
  final String name;
  final double lat;
  final double lon;

  Stop({
    required this.id,
    required this.name,
    required this.lat,
    required this.lon,
  });

  factory Stop.fromCsv(List<dynamic> row) {
    return Stop(
      id: row[0].toString(),
      name: row[2].toString(), // stops.txt usually has stop_name at column 2
      lat: double.parse(row[4].toString()),
      lon: double.parse(row[5].toString()),
    );
  }
}

/// Widget to display the list of GTFS stops
class StopsScreen extends StatefulWidget {
  const StopsScreen({Key? key}) : super(key: key);

  @override
  _StopsScreenState createState() => _StopsScreenState();
}

class _StopsScreenState extends State<StopsScreen> {
  List<Stop> stops = [];
  bool isLoading = true;
  String? errorMessage;

  @override
  void initState() {
    super.initState();
    fetchStops();
  }

  Future<void> fetchStops() async {
    const gtfsUrl = 'https://gtfs.sofiatraffic.bg/api/v1/static';

    try {
      final response = await http.get(Uri.parse(gtfsUrl));

      if (response.statusCode != 200) {
        throw Exception('Failed to download GTFS data');
      }

      // Decode ZIP
      final archive = ZipDecoder().decodeBytes(response.bodyBytes);

      // Find stops.txt
      final stopsFile = archive.firstWhere(
        (file) => file.name.toLowerCase() == 'stops.txt',
        orElse: () => throw Exception('stops.txt not found in ZIP'),
      );

      final stopsCsv = utf8.decode(stopsFile.content);
      final rows = csv.decode(stopsCsv);
      // Skip header and parse stops
      final parsedStops = rows.skip(1).map((row) => Stop.fromCsv(row)).toList();

      setState(() {
        stops = parsedStops;
        isLoading = false;
      });
    } catch (e) {
      setState(() {
        isLoading = false;
        errorMessage = e.toString();
      });
    }
  }

  @override
  Widget build(BuildContext context) {
    if (isLoading) {
      return Scaffold(
        appBar: AppBar(title: Text('Stops')),
        body: Center(child: CircularProgressIndicator()),
      );
    }

    if (errorMessage != null) {
      return Scaffold(
        appBar: AppBar(title: const Text('Stops')),
        body: Center(child: Text('Error: $errorMessage')),
      );
    }

    return Scaffold(
      appBar: AppBar(title: const Text('GTFS Stops')),
      body: ListView.builder(
        itemCount: stops.length,
        itemBuilder: (context, index) {
          final stop = stops[index];
          return ListTile(
            title: Text(stop.name),
            subtitle: Text('${stop.lat}, ${stop.lon}'),
          );
        },
      ),
    );
  }
}