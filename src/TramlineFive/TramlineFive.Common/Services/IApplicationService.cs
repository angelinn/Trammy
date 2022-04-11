﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TramlineFive.Common.Models;

namespace TramlineFive.Common.Services
{
    public interface IApplicationService
    {
        string GetStringSetting(string key, string defaultValue);
        bool GetBoolSetting(string key, bool defaultValue);
        void SetStringSetting(string key, string value);
        void SetBoolSetting(string key, bool value);


        string GetVersion();
        void OpenUri(string uri);
        void RunOnUIThread(Action action);
        Task<Position> GetCurrentPositionAsync();
        bool HasLocationPermissions();

        Task<bool> DisplayAlertAsync(string title, string message, string ok, string cancel = "");
        void DisplayToast(string message);
        void DisplayNotification(string title, string message);
        void VibrateShort();

    }
}
