import 'dart:core';

import 'package:flutter/material.dart';
import 'package:trammy/controllers/map_screen_controller.dart';
import 'package:trammy/models/gtfs/route.dart';
import 'package:trammy/services/common.dart';

class ArrivalCard extends StatelessWidget {
  final GTFSRoute route;
  final String direction;
  final List<ArrivalEntry> arrivals;

  ArrivalCard({super.key, required this.route, required this.direction, required this.arrivals});

  @override
  Widget build(BuildContext context) {
    return Card(
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
      elevation: 3,
      margin: const EdgeInsets.symmetric(vertical: 6),
      child: Padding(
        padding: const EdgeInsets.symmetric(vertical: 8, horizontal: 12),
        child: Row(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  CardHeader(route: route, direction: direction),
                  const SizedBox(height: 6),
                  // Upcoming times as chips
                  ArrivalTimes(arrivals: arrivals)
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }
}

class CardHeader extends StatelessWidget {
  final GTFSRoute route;
  final String direction;

  CardHeader({super.key, required this.route, required this.direction});

  @override
  Widget build(BuildContext context) {
    return Row(
      children: [
        Container(
          padding: const EdgeInsets.symmetric(
            vertical: 4,
            horizontal: 8,
          ),
          decoration: BoxDecoration(
            color: colorFromHex(route.routeColor!),
            borderRadius: BorderRadius.circular(6),
          ),
          child: Text(
            route.routeShortName!,
            style: const TextStyle(
              color: Colors.white,
              fontWeight: FontWeight.bold,
            ),
          ),
        ),
        const SizedBox(height: 10),
        Padding(
          padding: const EdgeInsets.only(left: 8.0),
          child: Text(
            direction,
            style: const TextStyle(fontSize: 14),
          ),
        ),
      ],
    );  
  }
}

class ArrivalTimes extends StatelessWidget {
  final List<ArrivalEntry> arrivals;

  const ArrivalTimes({required this.arrivals});

  @override
  Widget build(BuildContext context) {
    return Wrap(
      spacing: 6,
      children: arrivals
          .take(3)
          .map(
            (arrival) => Chip(
              label: Text("${arrival.arrival.difference(DateTime.now()).inMinutes.toString()} мин"),
              visualDensity: VisualDensity.compact,
              avatar: arrival.online
                ? const Icon(Icons.wifi, size: 16, color: Colors.green)
                : null,
            ),
          )
          .toList(),
    );
  }
}