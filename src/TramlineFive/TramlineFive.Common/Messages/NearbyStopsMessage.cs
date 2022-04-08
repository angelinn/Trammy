using SkgtService.Models.Locations;
using System;
using System.Collections.Generic;
using System.Text;

namespace TramlineFive.Common.Messages
{
    public class NearbyStopsMessage
    {
        public List<StopLocation> NearbyStops { get; set; }

        public NearbyStopsMessage(List<StopLocation> nearbyStops)
        {
            NearbyStops = nearbyStops;
        }
    }
}
