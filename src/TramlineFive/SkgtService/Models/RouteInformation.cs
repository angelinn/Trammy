﻿using SkgtService.Models.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkgtService.Models;

public class LineRoute
{
    public List<string> Codes { get; set; }

    public LineRoute(Way way)
    {
        Codes = way.Codes;
    }
}

public class RouteInformation
{
    public string Type { get; set; }
    public string LineName { get; set; }
    public List<LineRoute> Routes { get; set; }

    public RouteInformation(string type, string lineName, List<Way> routes)
    {
        Type = type;
        LineName = lineName.Replace("E", "Е").Replace("TM", "-ТМ");
        Routes = routes.Select(r => new LineRoute(r)).ToList();
    }
}
