using System;
using System.Collections.Generic;
using System.Text;

namespace TramlineFive.Common.Models;

public class ArrivalStopModel
{
    public string StopCode { get; set; }
    public string Name { get; set; }

    public ArrivalStopModel(string code, string name)
    {
        StopCode = code;
        Name = name;
    }
}
