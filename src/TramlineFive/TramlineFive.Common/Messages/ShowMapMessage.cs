using System;
using System.Collections.Generic;
using System.Text;

namespace TramlineFive.Common.Messages
{
    public class ShowMapMessage
    {
        public ShowMapMessage(bool show = true, int arrivalsCount = 0, long elapsedMilliseconds = 0)
        {
            ArrivalsCount = arrivalsCount;
            Show = show;
            ElapsedMilliseconds = elapsedMilliseconds;
        }

        public int ArrivalsCount { get; set; }
        public bool Show { get; set; }
        public long ElapsedMilliseconds { get; }
    }
}
