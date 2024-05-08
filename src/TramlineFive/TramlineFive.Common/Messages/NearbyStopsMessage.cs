using SkgtService.Models;
using SkgtService.Models.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace TramlineFive.Common.Messages
{
    public class NearbyStopsMessage
    {
        public List<StopInformation> NearbyStops { get; set; }

        public NearbyStopsMessage() { }

        public NearbyStopsMessage(List<StopInformation> nearbyStops)
        {
            NearbyStops = nearbyStops;
        }
    }
}
