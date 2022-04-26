using Plugin.Geolocator;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TramlineFive.Common.Models;
using TramlineFive.Common.Services;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace TramlineFive.Services.Main
{
    public class ApplicationService : IApplicationService
    {
        public string GetStringSetting(string key, string defaultValue)
        {
            return Preferences.Get(key, defaultValue);
        }

        public bool GetBoolSetting(string key, bool defaultValue)
        {
            return Preferences.Get(key, defaultValue);
        }

        public void SetStringSetting(string key, string value)
        {
            Preferences.Set(key, value);
        }

        public void SetBoolSetting(string key, bool value)
        {
            Preferences.Set(key, value);
        }

        public async Task<Position> GetCurrentPositionAsync()
        {
            GeolocationRequest request = new GeolocationRequest(GeolocationAccuracy.High, TimeSpan.FromSeconds(5));
            Location position = await Geolocation.GetLocationAsync(request);

            //Plugin.Geolocator.Abstractions.Position position = await CrossGeolocator.Current.GetPositionAsync(timeout: TimeSpan.FromSeconds(5));

            return new Position
            {
                Latitude = position.Latitude,
                Longitude = position.Longitude
            };
        }

        public void OpenLocationUI()
        {
            DependencyService.Get<IPermissionService>().OpenLocationSettingsPage();
        }

        public string GetVersion()
        {
            return AppInfo.VersionString;
        }

        public void VibrateShort()
        {
            Vibration.Vibrate(TimeSpan.FromMilliseconds(30));
        }

        public void OpenUri(string uri)
        {
            Device.OpenUri(new Uri(uri));
        }

        public async Task<bool> HasLocationPermissions()
        {
            return await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>() == PermissionStatus.Granted;
        }

        public void RunOnUIThread(Action action)
        {
            Device.BeginInvokeOnMainThread(action);
        }

        public async Task<bool> DisplayAlertAsync(string title, string message, string ok, string cancel)
        {
            if (!String.IsNullOrEmpty(cancel))
                return await Application.Current.MainPage.DisplayAlert(title, message, ok, cancel);

            await Application.Current.MainPage.DisplayAlert(title, message, ok);
            return true;
        }

        public void DisplayToast(string message)
        {
            Device.BeginInvokeOnMainThread(() => DependencyService.Get<IToastService>().ShowToast(message));
        }

        public void DisplayNotification(string title, string message)
        {
            DependencyService.Get<IPushService>().PushNotification(title, message);
        }

        public int GetIntSetting(string key, int defaultValue)
        {
            return Preferences.Get(key, defaultValue);
        }

        public void SetIntSetting(string key, int value)
        {
            Preferences.Set(key, value);
        }
    }
} 
