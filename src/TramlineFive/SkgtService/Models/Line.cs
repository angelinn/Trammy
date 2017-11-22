using System;
using System.Collections.Generic;
using System.Text;

namespace SkgtService.Models
{
    public class Line
    {
        public string Name { get; set; }
        public List<Arrival> Arrivals { get; set; }
    }
}
