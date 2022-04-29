using System;
using System.Collections.Generic;
using System.Text;

namespace SkgtService.Exceptions;

public class TramlineFiveException : Exception
{
    public TramlineFiveException(string message) : base(message)
    {   }
}
