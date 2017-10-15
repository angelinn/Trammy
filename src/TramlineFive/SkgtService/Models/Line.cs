using System;
using System.Collections.Generic;
using System.Text;

namespace SkgtService.Models
{
    public class Line
    {
        public string DisplayName { get; set; }
        public string SkgtValue { get; set; }

        public Line(string name, string value)
        {
            DisplayName = name;
            SkgtValue = value;
        }
    }
}
