using System;
using System.Collections.Generic;
using System.Text;

namespace TramlineFive.Common.Models;

public record Project(string Name, string License, string Url)
{
    public Project() : this("", "", "")
    {

    }
}

public class Projects
{
    public List<Project> Licenses { get; set; }
}
