using SkgtService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TramlineFive.Common.Models;

public class LineViewModel
{
    public TransportType Type { get; set; }
    public string Name { get; set; }
    public LineInformation Routes { get; set; }
}
