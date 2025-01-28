using ExCSS;
using SkgtService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TramlineFive.Common.Services;

public static class TransportConvertеr
{
    public const string DEFAULT_COLOR = "#EE3124";

    public static string TypeToColor(TransportType type, bool isLightTheme = true)
    {
        return type switch
        {
            TransportType.Bus => isLightTheme ? "#EE3124" : "#FFDC143C",
            TransportType.Tram => isLightTheme ? "#ffa500" : "#FFFF8c00",
            TransportType.Trolley  => isLightTheme ? "#2aa9e0" : "#FF00008B",
            TransportType.Electrobus => isLightTheme ? "#FF008000" : "#FF006400",
            TransportType.Additional => isLightTheme ? "#13b79d" : "#13b79d",
            TransportType.Subway=> "#0e6abc",
            TransportType.NightBus => "#2B3C8F",
            _ => "#EE3124"
        };
    }
}
