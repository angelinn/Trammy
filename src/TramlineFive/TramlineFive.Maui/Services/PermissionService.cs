using System;
using System.Collections.Generic;
using System.Text;

namespace TramlineFive.Maui.Services;

public partial class PermissionService
{
    public partial bool HasLocationPermissions();
    public partial void RequestLocationPermissions();
    public partial bool OpenLocationSettingsPage();
    public partial void ChangeNavigationBarColor(string color);
    public partial void ChangeStatusBarColor(string color);
}
