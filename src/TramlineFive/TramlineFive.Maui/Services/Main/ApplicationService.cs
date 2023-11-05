using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TramlineFive.Common.Models;
using TramlineFive.Common.Services;
using TramlineFive.Maui.Services;

namespace TramlineFive.Services.Main;

public class ApplicationService : IApplicationService
{
    private readonly PermissionService permissionService;
    private readonly PushService pushService;

    public ApplicationService(PermissionService permissionService, PushService pushService)
    {
        this.permissionService = permissionService;
        this.pushService = pushService;
    }

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

    public async Task<Position?> GetCurrentPositionAsync()
    {
        GeolocationRequest request = new GeolocationRequest(GeolocationAccuracy.High, TimeSpan.FromSeconds(5));
        try
        {
            Location position = await Geolocation.GetLocationAsync(request);
            if (position == null)
                return null;

            return new Position
            {
                Latitude = position.Latitude,
                Longitude = position.Longitude
            };
        }
        catch (FeatureNotEnabledException)
        {
            return null;
        }
    }

    public async Task<bool> RequestLocationPermissions()
    {
        return await Permissions.RequestAsync<Permissions.LocationWhenInUse>() == PermissionStatus.Granted;
    }

    public void OpenLocationUI()
    {
        permissionService.OpenLocationSettingsPage();
    }

    public string GetVersion()
    {
        return AppInfo.VersionString;
    }

    public void VibrateShort()
    {
        try
        {
            Vibration.Vibrate(TimeSpan.FromMilliseconds(30));
        }
        catch (FeatureNotSupportedException ex)
        {

        }
    }

    public async Task OpenUri(string uri)
    {
        await Launcher.OpenAsync(new Uri(uri));
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
        IToast toast = Toast.Make(message);
        toast.Show();
    }

    public void MakeSnack(string message)
    {
        SnackbarOptions options = new SnackbarOptions
        {
            BackgroundColor = Colors.DodgerBlue,
            TextColor = Colors.White,
            ActionButtonTextColor = Colors.White,
            CornerRadius = new CornerRadius(10),
            Font = Microsoft.Maui.Font.SystemFontOfSize(16),
            ActionButtonFont = Microsoft.Maui.Font.OfSize(null, 18, enableScaling: false),
        };

        ISnackbar snack = Snackbar.Make(message, visualOptions: options);

        snack.Show();
    }

    public void DisplayNotification(string title, string message)
    {
        pushService.PushNotification(title, message);
    }

    public int GetIntSetting(string key, int defaultValue)
    {
        return Preferences.Get(key, defaultValue);
    }

    public void SetIntSetting(string key, int value)
    {
        Preferences.Set(key, value);
    }

    public void ChangeNavigationBarColor(string color)
    {
        permissionService.ChangeNavigationBarColor(color);
    }

    public void ChangeStatusBarColor(string color)
    {
        permissionService.ChangeStatusBarColor(color);
    }
}
