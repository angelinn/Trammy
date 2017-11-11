using System;
using System.Collections.Generic;
using System.Text;

namespace SkgtService.Models
{
    public class StopInfo
    {
        public string Name { get; set; }
        public string Direction { get; set; }
        public List<SkgtObject> Lines { get; set; } = new List<SkgtObject>();
    }
}
