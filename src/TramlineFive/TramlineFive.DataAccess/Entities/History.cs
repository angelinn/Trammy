using System;
using System.Collections.Generic;
using System.Text;

namespace TramlineFive.DataAccess.Entities
{
    public class History
    {
        public int ID { get; set; } 
        public string Line { get; set; }
        public string StopCode { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}
