using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SkgtService.Models
{
    public class Line
    {
        public string Name { get; set; }
        public string Direction { get; set; }
        [JsonProperty("vehicle_type")]
        public string VehicleType { get; set; }
        public List<Arrival> Arrivals { get; set; }

        public string LastTimings => String.Join(", ", Arrivals.Take(3).Select(t => t.Time));
        public int Minutes
        {
            get
            {
                DateTime closest = DateTime.Parse(Arrivals[0].Time);
                return (int)Math.Round((closest - DateTime.Now).TotalMinutes);
            }
        }

        public string TransportType
        {
            get
            {
                switch (VehicleType)
                {
                    case "bus":
                        return "Автобус";
                    case "tram":
                        return "Трамвай";
                    case "trolley":
                        return "Тролей";

                    default:
                        return String.Empty;
                }
            }
        }
    }
}
