﻿using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using TramlineFive.Droid.Services;
using Android;
using TramlineFive.Common.Services;
using TramlineFive.Services;
using Android.Content;
using System.Reflection;
using Android.Util;
using TramlineFive.Common.Messages;

using Android.Content.Res;
using TramlineFive.Maui.Services;
using Bumptech.Glide.Load;
using TramlineFive.Maui.Platforms.Android;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using CommunityToolkit.Mvvm.Messaging;

namespace TramlineFive.Maui
{
    [Activity(Label = "Trammy", Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            //TabLayoutResource = Resource.Layout.Tabbar;
            //ToolbarResource = Resource.Layout.Toolbar;

            //base.Window.RequestFeature(WindowFeatures.ActionBar);
            //base.Window.RequestFeature(WindowFeatures.ActionModeOverlay);
            // Name of the MainActivity theme you had there before.
            // Or you can use global::Android.Resource.Style.ThemeHoloLight
            //base.SetTheme(Resource.Style.TramTheme);
            //ActionBar.Hide();
            //base.Window.RequestFeature(WindowFeatures./*ActionBarOverlay*/);

            base.OnCreate(bundle);
            //base.SetTheme(Resource.Style.TramTheme);
            //Window.SetFlags(WindowManagerFlags.TranslucentStatus, WindowManagerFlags.TranslucentStatus);
            Window.SetStatusBarColor(Android.Graphics.Color.Transparent); 
            var s = SystemUiFlags.LayoutFullscreen | SystemUiFlags.LayoutStable;
            FindViewById(Android.Resource.Id.Content).SystemUiVisibility = (StatusBarVisibility)s;

        //global::Xamarin.Forms.Forms.Init(this, bundle);
        //Xamarin.Essentials.Platform.Init(this, bundle); // add this line to your code, it may also be called: bundle

            PermissionService.Init(this);

            InitFontScale();
            WeakReferenceMessenger.Default.Register<ChangeThemeMessage>(this, OnThemeChanged);

            //LoadApplication(new App());

            AndroidEnvironment.UnhandledExceptionRaiser += delegate (object sender, RaiseThrowableEventArgs args) {
                //typeof(System.Exception).GetField("stack_trace", BindingFlags.NonPublic | BindingFlags.Instance)
                //    .SetValue(args.Exception, null);

                new AlertDialog.Builder(this).SetMessage(args.Exception.Message).Create().Show();
            };

            if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.PostNotifications) != Permission.Granted)
            {
                ActivityCompat.RequestPermissions(this, new[] { Manifest.Permission.PostNotifications }, 0);
            }
        }

        //protected override void OnResume()
        //{
        //    base.OnResume();
        //    Intent intent = new Intent(this, typeof(WatchService));
        //    StopService(intent);
        //}
        
        //protected override void OnPause()
        //{
        //    base.OnPause();

        //    Intent intent = new Intent(this, typeof(WatchService));
        //    intent.PutExtra("line", "107");
        //    intent.PutExtra("stop", "2193");

        //    if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
        //    {
        //        StartForegroundService(intent);
        //    }
        //    else
        //    {
        //        StartService(intent);
        //    }
        //}

        private void InitFontScale()
        {
            Resources.Configuration.FontScale = (float)1;
            //0.85 small, 1 standard, 1.15 big，1.3 more bigger ，1.45 supper big 
            DisplayMetrics metrics = new DisplayMetrics();
            WindowManager.DefaultDisplay.GetMetrics(metrics);
            metrics.ScaledDensity = Resources.Configuration.FontScale * metrics.Density;
            BaseContext.Resources.UpdateConfiguration(Resources.Configuration, metrics);
        }

        private void OnThemeChanged(object recipient, ChangeThemeMessage message)
        {
            Android.Graphics.Color color = message.Name == Common.Names.LightTheme ? Android.Graphics.Color.White : Android.Graphics.Color.ParseColor("#22272e");
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
                Window.SetNavigationBarColor(color);
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Android.Content.PM.Permission[] grantResults)
        {
            Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

    }
}

