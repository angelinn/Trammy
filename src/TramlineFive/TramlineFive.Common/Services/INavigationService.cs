using SkgtService.Models;
using SkgtService.Models.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TramlineFive.Common.Models;

namespace TramlineFive.Common.Services;

public interface INavigationService
{
    void ChangePage(string pageName);
    Task GoToDetails(Line line, string stop = "");
    void GoToDetails(ArrivalInformation line, string stop);
}
