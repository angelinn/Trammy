using CommunityToolkit.Mvvm.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Text;
using TramlineFive.DataAccess.Domain;

namespace TramlineFive.Common.Messages
{
    public class StopSelectedMessagePayload
    {
        public string Selected { get; set; }
        public bool Clicked { get; set; }

        public StopSelectedMessagePayload(string selected, bool clicked)
        {
            Selected = selected;
            Clicked = clicked;
        }
    }

    public class StopSelectedMessage : ValueChangedMessage<StopSelectedMessagePayload>
    {
        public StopSelectedMessage(StopSelectedMessagePayload value) : base(value)
        {
        }
    }
}
