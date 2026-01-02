using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkgtService.Models;
using SkgtService.Models.Json;
using TramlineFive.Common.Services.Interfaces;

namespace TramlineFive.Console
{
    public class DummyNavigationService : INavigationService
    {
        public void ChangePage(string pageName, Dictionary<string, object> payload = null)
        {

        }

        public Task GoToDetails(Line line, string stop = "")
        {
            throw new NotImplementedException();
        }

        public void GoToDetails(RouteArrivalInformation line, string stop)
        {

        }

        public Task GoToSchedule(RouteResponse route, string stopCode)
        {
            throw new NotImplementedException();
        }
    }
}
