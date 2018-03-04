using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TramlineFive.Common.Models;

namespace TramlineFive.Common.Services
{
    public interface IApplicationService
    {
        string GetVersion();
        void OpenUri(string uri);
        void RunOnUIThread(Action action);
        Task<Position> GetCurrentPositionAsync();
        bool HasLocationPermissions();
    }
}
