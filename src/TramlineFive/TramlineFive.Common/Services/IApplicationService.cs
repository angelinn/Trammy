using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TramlineFive.Common.Models;

namespace TramlineFive.Common.Services;

public interface IApplicationService
{
    string GetStringSetting(string key, string defaultValue);
    bool GetBoolSetting(string key, bool defaultValue);
    int GetIntSetting(string key, int defaultValue);
    void SetStringSetting(string key, string value);
    void SetBoolSetting(string key, bool value);
    void SetIntSetting(string key, int value);

    string GetVersion();
    Task OpenUri(string uri);
    Task OpenBrowserAsync(Uri uri);
    void RunOnUIThread(Action action);
    Task<Position?> GetCurrentPositionAsync();
    Task<bool> HasLocationPermissions();

    Task<bool> DisplayAlertAsync(string title, string message, string ok, string cancel = "");
    void DisplayToast(string message);
    void MakeSnack(string message);
    void DisplayNotification(string title, string message);
    void VibrateShort();
    void OpenLocationUI();
    Task<bool> RequestLocationPermissions();
    void ChangeNavigationBarColor(string color);
    void ChangeStatusBarColor(string color);
    string GetAppDataDirectory();
}
