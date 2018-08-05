using System;
using System.Collections.Generic;
using System.Text;

namespace SkgtService.Exceptions
{
    public class StopNotFoundException : Exception
    {
        public StopNotFoundException(string message = "") : base(message)
        {   }
    }
}
