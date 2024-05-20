using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TramlineFive.Maui.Services;

public partial class PermissionService
{
    public partial void RequestLocationPermissions()
    {

    }

    public partial bool HasLocationPermissions()
    {
        return false;
    }

    public partial bool OpenLocationSettingsPage()
    {
        return false;
    } 

    public partial void ChangeNavigationBarColor(string color)
    { 

    }


    public partial void ChangeStatusBarColor(string color)
    {

    }
}
