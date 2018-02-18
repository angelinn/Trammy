using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using TramlineFive.Droid.Services;
using TramlineFive.Services;
using Android;
using Android.Content.PM;

[assembly: Xamarin.Forms.Dependency(typeof(PermissionService))]
namespace TramlineFive.Droid.Services
{
    public class PermissionService : IPermissionService
    {
        private static Activity context;

        public static void Init(Activity mainActivity)
        {
            context = mainActivity;
        }

        public bool HasLocationPermissions()
        {
            return context.CheckSelfPermission(Manifest.Permission.AccessCoarseLocation) == Permission.Granted;
        }

        public void RequestLocationPermissions()
        {
            string[] PermissionsLocation =
            {
                    Manifest.Permission.AccessCoarseLocation,
                    Manifest.Permission.AccessFineLocation
                };
            context.RequestPermissions(PermissionsLocation, 0);
        }
    }
}