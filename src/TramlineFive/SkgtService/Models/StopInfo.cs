using System;
using System.Collections.Generic;
using System.Text;

namespace SkgtService.Models;

public class StopInfo
{
    public string Name { get; set; }
    public string Code { get; set; }
    public List<Line> Lines { get; set; }
    public bool IsFavourite { get; set; }
}
