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

namespace TramlineFive.Maui
{
    public partial class App : Application
    {
        private readonly PathService dbPathService;
        public App(PathService dbPathService)
        {
            InitializeComponent();

            this.dbPathService = dbPathService;
            TramlineFiveContext.DatabasePath = dbPathService.GetDBPath();

            MainPage = new AppShell();
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
            StopsLoader.Initialize(dbPathService.GetBaseFilePath());

            string theme = Preferences.Get(Settings.Theme, Names.SystemDefault);

            WeakReferenceMessenger.Default.Register<ChangeThemeMessage>(this, (r, m) => OnThemeChanged(m));

            WeakReferenceMessenger.Default.Send(new ChangeThemeMessage(theme));
            Current.RequestedThemeChanged += (s, a) =>
            {
                if (Preferences.Get(Settings.Theme, Names.SystemDefault) == Names.SystemDefault)
                    WeakReferenceMessenger.Default.Send(new ChangeThemeMessage(a.RequestedTheme == AppTheme.Light ? Names.LightTheme : Names.DarkTheme));
            };


            StopsLoader.OnStopsUpdated += OnStopsUpdated;

            IApplicationService applicationService = ServiceContainer.ServiceProvider.GetService<IApplicationService>();

            // Update stops every some time
            if (DateTime.TryParse(applicationService.GetStringSetting(Settings.StopsUpdated, null), out DateTime updated))
            {
                if (DateTime.Now - updated > TimeSpan.FromDays(7))
                {
                    System.Diagnostics.Debug.WriteLine($"Updating stops {DateTime.Now - updated} time old");

                    _ = StopsLoader.UpdateStopsAsync();
                    _ = StopsLoader.UpdateRoutesAsync();

                    applicationService.DisplayNotification("Trammy", "Спирките са обновени");
                }
            }

            WeakReferenceMessenger.Default.Register<SubscribeMessage>(this, (r, m) =>
            {
#if ANDROID
                if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.M)
                {
                    Android.Content.Intent batteryIntent = new();
                    string packageName = Platform.CurrentActivity.ApplicationContext.PackageName;
                    var pm = Platform.CurrentActivity.GetSystemService(Android.Content.Context.PowerService) as Android.OS.PowerManager;

                    if (!pm.IsIgnoringBatteryOptimizations(packageName))
                    {
                        batteryIntent.SetAction(Android.Provider.Settings.ActionRequestIgnoreBatteryOptimizations);
                        batteryIntent.SetData(Android.Net.Uri.Parse("package:" + packageName));
                        Platform.CurrentActivity.StartActivity(batteryIntent);
                    }
                }

                Android.App.ActivityManager activityManager = Platform.CurrentActivity.GetSystemService(Android.Content.Context.ActivityService) as Android.App.ActivityManager;
                if (activityManager.IsBackgroundRestricted)
                {

                    Android.Widget.Toast.MakeText(Platform.CurrentActivity, "Моля позволете приложението да работи във фонов режим.", Android.Widget.ToastLength.Long).Show();
                    Android.Content.Intent i = new Android.Content.Intent(Android.Provider.Settings.ActionApplicationDetailsSettings);

                    i.AddCategory(Android.Content.Intent.CategoryDefault);
                    i.SetData(Android.Net.Uri.Parse("package:" + Platform.CurrentActivity.ApplicationContext.PackageName));
                    Platform.CurrentActivity.StartActivity(i);

                    return;
                }

                Android.Content.Intent intent = new Android.Content.Intent(Platform.CurrentActivity, typeof(TramlineFive.Maui.Platforms.Android.WatchService));
                intent.PutExtra("line", m.lineName);
                intent.PutExtra("stop", m.stopCode);

                Platform.CurrentActivity.StopService(intent);

                if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O)
                {
                    Platform.CurrentActivity.StartForegroundService(intent);
                }
                else
                {
                    Platform.CurrentActivity.StartService(intent);
                }
#endif
            });
        }

        private void OnStopsUpdated(object sender, EventArgs e)
        {
            ServiceContainer.ServiceProvider.GetService<IApplicationService>().SetStringSetting(Settings.StopsUpdated, DateTime.Now.ToString());
        }
    }
}
