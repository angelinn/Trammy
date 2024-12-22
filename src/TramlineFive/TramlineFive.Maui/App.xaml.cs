using Microsoft.Maui.ApplicationModel;
using TramlineFive.Common.Messages;
using TramlineFive.Common;
using SkgtService;
using TramlineFive.Common.ViewModels;
using TramlineFive.DataAccess;
using TramlineFive.Services;
using TramlineFive.Common.Services;
using TramlineFive.Themes;
using TramlineFive.Maui.Services;
using TramlineFive.Pages;
using Microsoft.Maui.Controls.PlatformConfiguration;
using TramlineFive.Services.Main;
using CommunityToolkit.Mvvm.Messaging;
using SkgtService.Parsers;
using System.Diagnostics;
using Plugin.LocalNotification;
using SkgtService.Models.Json;

namespace TramlineFive.Maui
{
    public partial class App : Application
    {
        private readonly StopsLoader stopsLoader;
        private readonly PublicTransport publicTransport;

        public App(StopsLoader stopsLoader, PublicTransport publicTransport)
        {
            InitializeComponent();

            AppDomain.CurrentDomain.FirstChanceException += CurrentDomain_FirstChanceException;

            this.stopsLoader = stopsLoader;
            this.publicTransport = publicTransport;

            MainPage = new AppShell();
        }

        private void CurrentDomain_FirstChanceException(object sender, System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs e)
        {
            Debug.WriteLine($"***** Handling Unhandled Exception *****: {e.Exception.Message}");
        }

        private void OnThemeChanged(ChangeThemeMessage m)
        {
            System.Diagnostics.Debug.WriteLine("setting theme");
            ResourceDictionary themeDictionary = Current.Resources.MergedDictionaries.FirstOrDefault(d => d.ContainsKey("PrimaryColor"));
            if (themeDictionary != null)
                Current.Resources.MergedDictionaries.Remove(themeDictionary);

            if (m.Name == Names.SystemDefault)
            {
                //Current.UserAppTheme = Current.RequestedTheme;
                if (Current.RequestedTheme == AppTheme.Light)
                    Current.Resources.MergedDictionaries.Add(new LightTheme());
                else if (Current.RequestedTheme == AppTheme.Dark)
                    Current.Resources.MergedDictionaries.Add(new DarkTheme());
            }
            else if (m.Name == Names.LightTheme)
            {
                //Current.UserAppTheme = AppTheme.Light;
                Current.Resources.MergedDictionaries.Add(new LightTheme());
            }
            else
            {
                //Current.UserAppTheme = AppTheme.Dark;
                Current.Resources.MergedDictionaries.Add(new DarkTheme());
            }
            System.Diagnostics.Debug.WriteLine("theme set");
        }

        protected override void OnStart()
        {
            base.OnStart();

            TramlineFiveContext.DatabasePath = Path.Combine(FileSystem.AppDataDirectory, "tramlinefive.db");

            Task.Run(() =>
            {
                IApplicationService applicationService = ServiceContainer.ServiceProvider.GetService<IApplicationService>();

                string theme = Preferences.Get(Settings.Theme, Names.SystemDefault);
                WeakReferenceMessenger.Default.Register<ChangeThemeMessage>(this, (r, m) => OnThemeChanged(m));
                WeakReferenceMessenger.Default.Send(new ChangeThemeMessage(theme));
                Current.RequestedThemeChanged += (s, a) =>
                {
                    if (Preferences.Get(Settings.Theme, Names.SystemDefault) == Names.SystemDefault)
                        WeakReferenceMessenger.Default.Send(new ChangeThemeMessage(a.RequestedTheme == AppTheme.Light ? Names.LightTheme : Names.DarkTheme));
                };

                StopsLoader.OnStopsUpdated += OnStopsUpdated;

                _ = publicTransport.LoadData();

                _ = App.Current.Handler.MauiContext.Services.GetService<HistoryViewModel>().LoadHistoryAsync();
                _ = App.Current.Handler.MauiContext.Services.GetService<FavouritesViewModel>().LoadFavouritesAsync();

                //_ = ServiceContainer.ServiceProvider.GetService<HistoryViewModel>().LoadHistoryAsync();
                //_ = ServiceContainer.ServiceProvider.GetService<FavouritesViewModel>().LoadFavouritesAsync();

                _ = UpdateStopsIfOlderThanAsync(TimeSpan.FromDays(7));

              
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

        private async Task UpdateStopsIfOlderThanAsync(TimeSpan timespan)
        {
            IApplicationService applicationService = ServiceContainer.ServiceProvider.GetService<IApplicationService>();

            // Update stops every some time
            if (DateTime.TryParse(applicationService.GetStringSetting(Settings.StopsUpdated, null), out DateTime updated))
            {
                if (DateTime.Now - updated > timespan)
                {
                    Debug.WriteLine($"Updating stops {DateTime.Now - updated} time old");

                    await stopsLoader.UpdateStopsAsync();
                    //_ = StopsLoader.UpdateRoutesAsync();

                    await applicationService.DisplayNotification("Trammy", "Спирките са обновени");
                }
            }
        }

        private void OnStopsUpdated(object sender, string newVersion)
        {
            ServiceContainer.ServiceProvider.GetService<IApplicationService>().SetStringSetting("APIVersion", newVersion);
            ServiceContainer.ServiceProvider.GetService<IApplicationService>().SetStringSetting(Settings.StopsUpdated, DateTime.Now.ToString());
        }
    }
}
