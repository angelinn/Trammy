using System;
using System.Collections.Generic;
using System.Text;
using TramlineFive.DataAccess.Domain;

namespace TramlineFive.Common.Messages
{
    public class StopSelectedMessage
    {
        public string Selected { get; set; }

        public StopSelectedMessage(string selected)
        {
            Selected = selected;
        }
    }
}
