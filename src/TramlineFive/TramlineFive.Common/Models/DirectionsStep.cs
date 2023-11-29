using SkgtService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;   

namespace TramlineFive.Common.Models;

public class DirectionsStep
{
    public StopInformation FromStop { get; set; }
    public LineInformation FromLine { get; set; }

    public StopInformation ToStop { get; set; }
    public LineInformation ToLine { get; set; }
}
