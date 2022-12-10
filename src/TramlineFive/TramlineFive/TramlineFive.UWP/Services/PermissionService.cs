using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TramlineFive.Services;
using TramlineFive.UWP.Services;

[assembly: Xamarin.Forms.Dependency(typeof(PermissionService))]
namespace TramlineFive.UWP.Services
{

    public class PermissionService : IPermissionService
    {
        public void ChangeNavigationBarColor(string color)
        {

        }

        public void ChangeStatusBarColor(string color)
        {

        }

        public bool HasLocationPermissions()
        {
            return true;
        }

        public bool OpenLocationSettingsPage()
        {
            return true;
        }

        public void RequestLocationPermissions()
        {

        }
    }
}
