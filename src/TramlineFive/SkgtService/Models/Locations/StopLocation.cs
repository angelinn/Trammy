using System;
using System.Collections.Generic;
using System.Text;

namespace SkgtService.Models.Locations
{
    public class StopLocation
    {
        public double Lat { get; set; }
        public double Lon { get; set; }
        public string PublicName { get; set; }
        public string Code { get; set; }
    }
}
