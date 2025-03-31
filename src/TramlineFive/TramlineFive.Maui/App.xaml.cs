using Microsoft.Maui.ApplicationModel;
using TramlineFive.Common.Messages;
using TramlineFive.Common;
using SkgtService;
using TramlineFive.Common.ViewModels;
using TramlineFive.DataAccess;
using TramlineFive.Services;
using TramlineFive.Common.Services;
using TramlineFive.Maui.Services;
using TramlineFive.Pages;
using Microsoft.Maui.Controls.PlatformConfiguration;
using TramlineFive.Services.Main;
using CommunityToolkit.Mvvm.Messaging;
using SkgtService.Parsers;
using System.Diagnostics;
using Plugin.LocalNotification;
using SkgtService.Models.Json;
using TramlineFive.Common.Services.Interfaces;
using CommunityToolkit.Maui.Alerts;
using Microsoft.Maui.Storage;
using NetTopologySuite.Operation.Overlay.Validate;
using System.Runtime.InteropServices.Marshalling;

namespace TramlineFive.Maui
{
    public partial class App : Application
    {
        private readonly PublicTransport publicTransport;

        public App(PublicTransport publicTransport)
        {
            InitializeComponent();

            AppDomain.CurrentDomain.FirstChanceException += CurrentDomain_FirstChanceException;

            this.publicTransport = publicTransport;

            string theme = Preferences.Get(Settings.Theme, Names.SystemDefault);
            Application.Current.UserAppTheme = theme switch
            {
                Names.LightTheme => AppTheme.Light,
                Names.DarkTheme => AppTheme.Dark,
                _ => AppTheme.Unspecified
            };
        }

        protected override Window CreateWindow(IActivationState activationState)
        {
            return new Window(new AppShell());
        }

        private void CurrentDomain_FirstChanceException(object sender, System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs e)
        {
            Debug.WriteLine($"***** Handling Unhandled Exception *****: {e.Exception.Message}");
        }

        private void OnThemeChanged(ChangeThemeMessage m)
        {
            Application.Current.UserAppTheme = m.Name switch
            {
                Names.LightTheme => AppTheme.Light,
                Names.DarkTheme => AppTheme.Dark,
                _ => AppTheme.Unspecified
            };
        }

        protected override void OnStart()
        {
            base.OnStart();

            TramlineFiveContext.DatabasePath = Path.Combine(FileSystem.AppDataDirectory, "tramlinefive.db");

            Task.Run(() =>
            {
                IApplicationService applicationService = ServiceContainer.ServiceProvider.GetService<IApplicationService>();

                WeakReferenceMessenger.Default.Register<ChangeThemeMessage>(this, (r, m) => OnThemeChanged(m));
                WeakReferenceMessenger.Default.Register<ViewportChangedMessage>(this, (r, m) => OnViewportChanged(m));

                StopsLoader.OnStopsUpdated += OnStopsUpdated;

                publicTransport.Initialize(applicationService.GetStringSetting(Settings.StopsUpdated, null), TimeSpan.FromDays(3));

                _ = publicTransport.LoadData();

                ServiceContainer.ServiceProvider.GetService<HistoryViewModel>().LoadHistoryAsync().ContinueWith(
                    _ => ServiceContainer.ServiceProvider.GetService<FavouritesViewModel>().LoadFavouritesAsync()
                );

                _ = CheckForUpdateAsync();
            });
        }

        private async Task CheckForUpdateAsync()
        {
            DateTime versionCheckTime = Preferences.Get("VersionCheckDate", DateTime.MinValue);
            if (DateTime.Now - versionCheckTime < TimeSpan.FromDays(1))
                return;

            NewVersion version = await ServiceContainer.ServiceProvider.GetService<VersionService>().CheckForUpdates();
            if (version != null)
            {
                bool result = await MainPage.DisplayAlert("Нова версия", $"{AppInfo.Name} има нова версия {version.VersionNumber} 🎉", "СВАЛЯНЕ", "ОТКАЗ");
                if (result)
                {
                    Uri url = new Uri(version.ReleaseUrl);
                    await Browser.Default.OpenAsync(url);
                }
            }

            Preferences.Set("VersionCheckDate", DateTime.Now);
        }


        //            WeakReferenceMessenger.Default.Register<SubscribeMessage>(this, (r, m) =>
        //            {
        //#if ANDROID
        //                if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.M)
        //                {
        //                    Android.Content.Intent batteryIntent = new();
        //                    string packageName = Platform.CurrentActivity.ApplicationContext.PackageName;
        //                    var pm = Platform.CurrentActivity.GetSystemService(Android.Content.Context.PowerService) as Android.OS.PowerManager;

        //                    if (!pm.IsIgnoringBatteryOptimizations(packageName))
        //                    {
        //                        batteryIntent.SetAction(Android.Provider.Settings.ActionRequestIgnoreBatteryOptimizations);
        //                        batteryIntent.SetData(Android.Net.Uri.Parse("package:" + packageName));
        //                        Platform.CurrentActivity.StartActivity(batteryIntent);
        //                    }
        //                }

        //                Android.App.ActivityManager activityManager = Platform.CurrentActivity.GetSystemService(Android.Content.Context.ActivityService) as Android.App.ActivityManager;
        //                if (activityManager.IsBackgroundRestricted)
        //                {

        //                    Android.Widget.Toast.MakeText(Platform.CurrentActivity, "Моля позволете приложението да работи във фонов режим.", Android.Widget.ToastLength.Long).Show();
        //                    Android.Content.Intent i = new Android.Content.Intent(Android.Provider.Settings.ActionApplicationDetailsSettings);

        //                    i.AddCategory(Android.Content.Intent.CategoryDefault);
        //                    i.SetData(Android.Net.Uri.Parse("package:" + Platform.CurrentActivity.ApplicationContext.PackageName));
        //                    Platform.CurrentActivity.StartActivity(i);

        //                    return;
        //                }

        //                Android.Content.Intent intent = new Android.Content.Intent(Platform.CurrentActivity, typeof(TramlineFive.Maui.Platforms.Android.WatchService));
        //                intent.PutExtra("line", m.lineName);
        //                intent.PutExtra("stop", m.stopCode);

        //                Platform.CurrentActivity.StopService(intent);

        //                if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O)
        //                {
        //                    Platform.CurrentActivity.StartForegroundService(intent);
        //                }
        //                else
        //                {
        //                    Platform.CurrentActivity.StartService(intent);
        //                }
        //#endif
        //            });
        //}

        private void OnStopsUpdated(object sender, string newVersion)
        {
            IApplicationService applicationService = ServiceContainer.ServiceProvider.GetService<IApplicationService>();

            applicationService.SetStringSetting("APIVersion", newVersion);
            applicationService.SetStringSetting(Settings.StopsUpdated, DateTime.Now.ToString());

            if (!VersionTracking.IsFirstLaunchEver)
                Dispatcher.Dispatch(() => Toast.Make("Спирките са обновени").Show());
            
            WeakReferenceMessenger.Default.Send(new RefreshStopsMessage());

            //await applicationService.DisplayNotification("Trammy", "Спирките са обновени");
        }

        private void OnViewportChanged(ViewportChangedMessage m)
        {
            Preferences.Set(Settings.MapCenterX, m.x);
            Preferences.Set(Settings.MapCenterY, m.y);
        }
    }
}
