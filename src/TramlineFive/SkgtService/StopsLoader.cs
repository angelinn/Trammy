using Newtonsoft.Json;
using SkgtService.Models.Locations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SkgtService
{
    public class StopsLoader
    {
        public List<StopLocation> LoadStops(Stream stream)
        {
            string json = String.Empty;
            using (StreamReader reader = new StreamReader(stream))
            {
                json = reader.ReadToEnd();
            }
            return JsonConvert.DeserializeObject<List<StopLocation>>(json);
        }
    }
}
