using System;
using System.Collections.Generic;
using System.Text;

namespace TramlineFive.Common.Messages
{
    public class MapClickedResponseMessage
    {
        public MapClickedResponseMessage(bool handled)
        {
            Handled = handled;
        }

        public bool Handled { get; set; }
    }
}
