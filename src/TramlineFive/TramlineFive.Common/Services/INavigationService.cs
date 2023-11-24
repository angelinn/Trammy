using SkgtService.Models;
using SkgtService.Models.Json;
using System;
using System.Collections.Generic;
using System.Text;
using TramlineFive.Common.Models;

namespace TramlineFive.Common.Services;

public interface INavigationService
{
    void ChangePage(string pageName);
    void GoToDetails(LineViewModel line, string stop = "");
    void GoToDetails(ArrivalInformation line, string stop);
}
