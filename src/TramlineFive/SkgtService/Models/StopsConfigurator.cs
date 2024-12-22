using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkgtService.Models;

public class StopsConfigurator
{
    public string DatabasePath { get; set; }
    public string StopsVersion { get; set; }

    public StopsConfigurator(string databasePath, string stopsVersion)
    {
        DatabasePath = databasePath;
        StopsVersion = stopsVersion;
    }
}
