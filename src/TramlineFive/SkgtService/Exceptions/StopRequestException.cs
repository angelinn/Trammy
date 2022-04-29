using System;
using System.Collections.Generic;
using System.Text;

namespace SkgtService.Exceptions;

public class StopRequestException : Exception
{
    public StopRequestException(string message = "") : base(message)
    {   }
}
