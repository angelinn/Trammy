using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TramlineFive.Common.Services.Interfaces;

namespace TramlineFive.Console
{

    class DummyApplicationService : IApplicationService
    {
        public void ChangeNavigationBarColor(string color)
        {

        }

        public Task<bool> DisplayAlertAsync(string title, string message, string ok, string cancel = "")
        {
            return Task.FromResult(true);
        }

        public Task<int> DisplayNotification(string title, string message)
        {
            return Task.FromResult(0);
        }

        public void DisplayToast(string message)
        {

        }

        public string GetAppDataDirectory()
        {
            return "";
        }

        public bool GetBoolSetting(string key, bool defaultValue)
        {
            return true;
        }

        public Task<TramlineFive.Common.Models.Position?> GetCurrentPositionAsync()
        {
            return Task.FromResult<TramlineFive.Common.Models.Position?>(null);
        }

        public int GetIntSetting(string key, int defaultValue)
        {
            return 0;
        }

        public string GetStringSetting(string key, string defaultValue)
        {
            return "";
        }

        public string GetVersion()
        {
            return "1.0.0";
        }

        public Task<bool> HasLocationPermissions()
        {
            return Task.FromResult(true);
        }

        public void MakeSnack(string message)
        {

        }

        public Task OpenBrowserAsync(Uri uri)
        {
            return Task.CompletedTask;
        }

        public void OpenLocationUI()
        {

        }

        public Task<Stream> OpenResourceFileAsync(string fileName)
        {
            throw new NotImplementedException();
        }

        public Task OpenUri(string uri)
        {
            return Task.CompletedTask;
        }

        public Task<bool> RequestLocationPermissions()
        {
            return Task.FromResult(true);
        }

        public void ResetStatusBarStyle()
        {

        }

        public void RunOnUIThread(Action action)
        {

        }

        public void SetBoolSetting(string key, bool value)
        {

        }

        public void SetIntSetting(string key, int value)
        {

        }

        public void SetStringSetting(string key, string value)
        {

        }

        public void StopArrivalSub()
        {

        }

        public void SubscribeForArrival(string tripId, string stopId)
        {

        }

        public Task<bool> SubscribeForLocationChangeAsync(Action<TramlineFive.Common.Models.Position> action)
        {
            throw new NotImplementedException();
        }

        public void VibrateShort()
        {

        }
    }

}
