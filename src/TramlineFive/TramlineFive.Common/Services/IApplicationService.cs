using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TramlineFive.Common.Models;

namespace TramlineFive.Common.Services
{
    public interface IApplicationService
    {
        IDictionary<string, object> Properties { get; }
        string GetVersion();
        void OpenUri(string uri);
        void RunOnUIThread(Action action);
        Task<Position> GetCurrentPositionAsync();
        bool HasLocationPermissions();

        Task<bool> DisplayAlertAsync(string title, string message, string ok, string cancel = "");
        void DisplayToast(string message);
    }
}
