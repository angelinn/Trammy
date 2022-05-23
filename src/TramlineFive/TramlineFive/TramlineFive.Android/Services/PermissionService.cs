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
using Android.Graphics;

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
            if ((int)Build.VERSION.SdkInt < 23)
                return true;

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

        public bool OpenLocationSettingsPage()
        {
            var intent = new Intent(Android.Provider.Settings.ActionLocationSourceSettings);
            intent.AddFlags(ActivityFlags.NewTask);
            Application.Context.StartActivity(intent);
            return true;
        } 

        public void ChangeNavigationBarColor(string color)
        {
            Color androidColor = String.IsNullOrEmpty(color) ? Color.Transparent : Color.ParseColor(color);

            context.Window.SetNavigationBarColor(androidColor);
        }

        private int? originalColor;

        public void ChangeStatusBarColor(string color)
        {
            context.Window.ClearFlags(WindowManagerFlags.TranslucentStatus);
            context.Window.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);

            if (!originalColor.HasValue)
                originalColor = context.Window.StatusBarColor;

            Color androidColor = String.IsNullOrEmpty(color) ? new Color(originalColor.Value) : Color.ParseColor(color);

            //context.Window.StatusBarColor = color;

            context.Window.SetStatusBarColor(androidColor);
        }
    }
}
