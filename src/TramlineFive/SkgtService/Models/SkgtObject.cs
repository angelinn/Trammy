using System;
using System.Collections.Generic;
using System.Text;

namespace SkgtService.Models
{
    public class SkgtObject
    {
        public string DisplayName { get; set; }
        public string SkgtValue { get; set; }

        public SkgtObject(string name, string value)
        {
            DisplayName = name;
            SkgtValue = value;
        }
    }
}
