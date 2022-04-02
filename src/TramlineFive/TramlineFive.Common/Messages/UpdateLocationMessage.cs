using System;
using System.Collections.Generic;
using System.Text;
using TramlineFive.Common.Models;

namespace TramlineFive.Common.Messages
{
    public class UpdateLocationMessage
    {
        public Position Position { get; set; }

        public UpdateLocationMessage(Position position)
        {
            Position = position;
        }
    }
}
