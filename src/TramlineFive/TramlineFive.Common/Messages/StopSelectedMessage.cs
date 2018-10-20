using System;
using System.Collections.Generic;
using System.Text;
using TramlineFive.DataAccess.Domain;

namespace TramlineFive.Common.Messages
{
    public class StopSelectedMessage
    {
        public string Selected { get; set; }
        public bool Clicked { get; set; }

        public StopSelectedMessage(string selected, bool clicked)
        {
            Selected = selected;
            Clicked = clicked;
        }
    }
}
