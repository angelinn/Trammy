using System;
using System.Collections.Generic;
using System.Text;

namespace TramlineFive.Services;

public interface IPermissionService
{
    bool HasLocationPermissions();
    void RequestLocationPermissions();
    bool OpenLocationSettingsPage();
    void ChangeNavigationBarColor(string color);
    void ChangeStatusBarColor(string color);
}
