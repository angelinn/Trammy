using System;
using System.Collections.Generic;
using System.Text;

namespace TramlineFive.Common.Models
{
    public struct Position
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public Position(double latitude, double longitude) 
        {
            Latitude = latitude;
            Longitude = longitude;
        }
    }
}
