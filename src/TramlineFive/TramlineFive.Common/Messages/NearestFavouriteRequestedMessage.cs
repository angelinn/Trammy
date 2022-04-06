using System;
using System.Collections.Generic;
using System.Text;
using TramlineFive.Common.Models;

namespace TramlineFive.Common.Messages
{
    public class NearestFavouriteRequestedMessage
    {
        public Position Position { get; set; }

        public NearestFavouriteRequestedMessage(Position position)
        {
            Position = position;
        }
    } 
} 
