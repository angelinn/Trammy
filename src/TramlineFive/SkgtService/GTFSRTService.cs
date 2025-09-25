using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;
using TransitRealtime;

namespace SkgtService
{
    public class GTFSRTService
    {
        private readonly string TripUpdatesUrl;
        private readonly string VehicleUpdatesUrl;
        private readonly string AlertsUrl;

        public GTFSRTService(string tripUpdatesUrl, string vehicleUpdatesUrl, string alertsUrl)
        {
            TripUpdatesUrl = tripUpdatesUrl;
            VehicleUpdatesUrl = vehicleUpdatesUrl;
            AlertsUrl = alertsUrl;
        }

        public async Task<FeedMessage> FetchTripUpdates()
        {
            return await FetchGtfsRealtimeFeedAsync(TripUpdatesUrl);
        }

        public async Task<FeedMessage> FetchVehicleUpdates()
        {
            return await FetchGtfsRealtimeFeedAsync(VehicleUpdatesUrl);
        }

        public async Task<FeedMessage> FetchAlerts()
        {
            return await FetchGtfsRealtimeFeedAsync(AlertsUrl);
        }

        private async Task<FeedMessage> FetchGtfsRealtimeFeedAsync(string gtfsRtUrl)
        {
            using HttpClient httpClient = new HttpClient();
            HttpResponseMessage response = await httpClient.GetAsync(gtfsRtUrl);
            response.EnsureSuccessStatusCode();

            byte[] feedData = await response.Content.ReadAsByteArrayAsync();
           return Serializer.Deserialize<FeedMessage>(new System.IO.MemoryStream(feedData));
        }
    }
}
