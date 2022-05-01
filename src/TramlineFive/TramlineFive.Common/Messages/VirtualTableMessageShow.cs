using System;
using System.Collections.Generic;
using System.Text;

namespace TramlineFive.Common.Messages
{
    public class VirtualTableMessageShow
    {
        public VirtualTableMessageShow(bool show)
        {
            Show = show;
        }

        public bool Show { get; set; }
    }
}
