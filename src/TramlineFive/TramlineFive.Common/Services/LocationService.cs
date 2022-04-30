using System;
using System.Collections.Generic;
using System.Text;

namespace TramlineFive.Common.Services;

public class LocationService
{
    private const double R = 6378.137; // Radius of earth in KM

    public double GetDistance(double oneLat, double oneLon, double otherLat, double otherLon)
    {
        double dLat = otherLat * Math.PI / 180 - oneLat * Math.PI / 180;
        double dLon = otherLon * Math.PI / 180 - oneLon * Math.PI / 180;

        double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) + Math.Cos(oneLat * Math.PI / 180) * Math.Cos(otherLat * Math.PI / 180) * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        double d = (R * c) * 1000;

        return d;
    }
}
