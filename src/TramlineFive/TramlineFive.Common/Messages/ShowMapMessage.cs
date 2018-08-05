using System;
using System.Collections.Generic;
using System.Text;

namespace TramlineFive.Common.Messages
{
    public class ShowMapMessage
    {
        public ShowMapMessage(bool show = true, int arrivalsCount = 0)
        {
            ArrivalsCount = arrivalsCount;
            Show = show;
        }

        public int ArrivalsCount { get; set; }
        public bool Show { get; set; }
    }
}
