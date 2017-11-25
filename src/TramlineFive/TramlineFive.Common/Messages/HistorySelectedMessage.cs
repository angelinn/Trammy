using System;
using System.Collections.Generic;
using System.Text;
using TramlineFive.DataAccess.Domain;

namespace TramlineFive.Common.Messages
{
    public class HistorySelectedMessage
    {
        public HistoryDomain Selected { get; set; }

        public HistorySelectedMessage(HistoryDomain selected)
        {
            Selected = selected;
        }
    }
}
