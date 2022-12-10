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

namespace TramlineFive.Maui
{
    public partial class App : Application
    {
        private readonly PathService dbPathService;
        public App(PathService dbPathService, VersionCheckingService versionCheckingService)
        {
            InitializeComponent();

            this.dbPathService = dbPathService;

            StopsLoader.Initialize(dbPathService.GetBaseFilePath());

            string theme = Preferences.Get(Settings.Theme, Names.LightTheme);

            Messenger.Default.Register<ChangeThemeMessage>(this, m => OnThemeChanged(m));

            Messenger.Default.Send(new ChangeThemeMessage(theme));
            versionCheckingService.CreateTask();

            MainPage = new NavigationPage(new MainPage());// new AppShell();
        }


        private void OnThemeChanged(ChangeThemeMessage m)
        {
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
        }

        protected override async void OnStart()
        {
            //AppCenter.Start("android=e3e6e5fc-9b54-46ae-ad7d-8fe6b80e9471",
            //    typeof(Analytics), typeof(Crashes));

            //if (VersionTracking.IsFirstLaunchEver && await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>() != PermissionStatus.Granted)
            //    MainPage = new Pages.LocationPromptPage();
            //else
                //MainPage = new NavigationPage(new MainPage());

            TramlineFiveContext.DatabasePath = dbPathService.GetDBPath();
            await TramlineFiveContext.EnsureCreatedAsync();

            await ServiceContainer.ServiceProvider.GetService<HistoryViewModel>().LoadHistoryAsync();
            await ServiceContainer.ServiceProvider.GetService<FavouritesViewModel>().LoadFavouritesAsync();

            StopsLoader.OnStopsUpdated += OnStopsUpdated;
        }

        private void OnStopsUpdated(object sender, EventArgs e)
        {
            ServiceContainer.ServiceProvider.GetService<IApplicationService>().SetStringSetting(Settings.StopsUpdated, DateTime.Now.ToString());
        }
    }
}
