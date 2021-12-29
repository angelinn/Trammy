using System;
using System.Collections.Generic;
using System.Text;

namespace TramlineFive.Common.Messages
{
    public class SearchFocusedMessage
    {
        public bool Focused { get; set; }

        public SearchFocusedMessage(bool focused)
        {
            Focused = focused;
        }
    }
}
