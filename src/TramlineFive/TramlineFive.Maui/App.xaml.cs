using Microsoft.Maui.ApplicationModel;
using TramlineFive.Common.Messages;
using TramlineFive.Common;
using SkgtService;
using TramlineFive.Common.ViewModels;
using TramlineFive.DataAccess;
using TramlineFive.Services;
using TramlineFive.Common.Services;
using TramlineFive.Themes;
using GalaSoft.MvvmLight.Messaging;
using TramlineFive.Maui.Services;
using TramlineFive.Pages;

namespace TramlineFive.Maui
{
    public partial class App : Application
    {
        private readonly PathService dbPathService;
        public App(PathService dbPathService, VersionCheckingService versionCheckingService)
        {
            InitializeComponent();

            System.Diagnostics.Debug.WriteLine("Initialized");
            this.dbPathService = dbPathService;

            StopsLoader.Initialize(dbPathService.GetBaseFilePath());

            System.Diagnostics.Debug.WriteLine("Initialized stops loader");
            string theme = Preferences.Get(Settings.Theme, Names.LightTheme);

            Messenger.Default.Register<ChangeThemeMessage>(this, m => OnThemeChanged(m));

            Messenger.Default.Send(new ChangeThemeMessage(theme));
            System.Diagnostics.Debug.WriteLine("creating task");
            versionCheckingService.CreateTask();

            System.Diagnostics.Debug.WriteLine("created");
            MainPage = new AppShell();
        }


        private void OnThemeChanged(ChangeThemeMessage m)
        {
            System.Diagnostics.Debug.WriteLine("setting theme");
            ResourceDictionary themeDictionary = Current.Resources.MergedDictionaries.FirstOrDefault(d => d.ContainsKey("PrimaryColor"));
            if (themeDictionary != null)
                Current.Resources.MergedDictionaries.Remove(themeDictionary);

            if (m.Name == Names.LightTheme)
            {
                Current.UserAppTheme = AppTheme.Light;
                Current.Resources.MergedDictionaries.Add(new LightTheme());
            }
            else
            {
                Current.UserAppTheme = AppTheme.Dark;
                Current.Resources.MergedDictionaries.Add(new DarkTheme());
            }
            System.Diagnostics.Debug.WriteLine("theme set");
        }

        protected override async void OnStart()
        {
            //AppCenter.Start("android=e3e6e5fc-9b54-46ae-ad7d-8fe6b80e9471",
            //    typeof(Analytics), typeof(Crashes));

            //if (VersionTracking.IsFirstLaunchEver && await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>() != PermissionStatus.Granted)
            //    MainPage = new Pages.LocationPromptPage();
            //else
            //MainPage = new NavigationPage(new MainPage());

            System.Diagnostics.Debug.WriteLine("loading db");
            TramlineFiveContext.DatabasePath = dbPathService.GetDBPath();
            await TramlineFiveContext.EnsureCreatedAsync();

            //System.Diagnostics.Debug.WriteLine("loaded db");

            //System.Diagnostics.Debug.WriteLine("loading history");
            await ServiceContainer.ServiceProvider.GetService<HistoryViewModel>().LoadHistoryAsync();

            //System.Diagnostics.Debug.WriteLine("loaded history");

            //System.Diagnostics.Debug.WriteLine("loading favourites");
            await ServiceContainer.ServiceProvider.GetService<FavouritesViewModel>().LoadFavouritesAsync();
            //System.Diagnostics.Debug.WriteLine("loaded favourites");

            StopsLoader.OnStopsUpdated += OnStopsUpdated;
        }

        private void OnStopsUpdated(object sender, EventArgs e)
        {
            ServiceContainer.ServiceProvider.GetService<IApplicationService>().SetStringSetting(Settings.StopsUpdated, DateTime.Now.ToString());
        }
    }
}
