using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace SkgtService.Models.Json;

public class StopInfo
{
    public string Name { get; set; }
    public string Code { get; set; }
    public List<Line> Lines { get; set; }
}
