using System;
using System.Collections.Generic;
using System.Text;

namespace SkgtService.Models
{
    public class Stop
    {
        public string Name { get; set; }
        public string Direction { get; set; }
        public List<Line> Lines { get; set; } = new List<Line>();
    }
}
