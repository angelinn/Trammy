using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Extensions.DependencyInjection;
using SkgtService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TramlineFive.Common;
using TramlineFive.Common.Messages;
using TramlineFive.Common.Services;
using TramlineFive.Common.ViewModels;
using TramlineFive.DataAccess;
using TramlineFive.Services;
using TramlineFive.Services.Main;
using TramlineFive.Themes;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace TramlineFive
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            Startup.ConfigureServices();

            IPathService dbPathService = DependencyService.Get<IPathService>();
            StopsLoader.Initialize(dbPathService.BaseFilePath);

            string theme = Preferences.Get(Settings.Theme, Names.LightTheme);

            Messenger.Default.Register<ChangeThemeMessage>(this, m => OnThemeChanged(m));

            Messenger.Default.Send(new ChangeThemeMessage(theme));
            DependencyService.Get<IVersionCheckingService>().CreateTask();
        }

        private void OnThemeChanged(ChangeThemeMessage m)
        {
            ResourceDictionary themeDictionary = Current.Resources.MergedDictionaries.FirstOrDefault(d => d.ContainsKey("PrimaryColor"));
            if (themeDictionary != null)
                Current.Resources.MergedDictionaries.Remove(themeDictionary);

            if (m.Name == Names.LightTheme)
            {
                Current.UserAppTheme = OSAppTheme.Light;
                Current.Resources.MergedDictionaries.Add(new LightTheme());
            }
            else
            {
                Current.UserAppTheme = OSAppTheme.Dark;
                Current.Resources.MergedDictionaries.Add(new DarkTheme());
            }
        }

        protected override async void OnStart()
        {
            if (VersionTracking.IsFirstLaunchEver && await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>() != PermissionStatus.Granted)
                MainPage = new Pages.LocationPromptPage();
            else
                MainPage = new NavigationPage(new MasterPage());

            IPathService dbPathService = DependencyService.Get<IPathService>();
            TramlineFiveContext.DatabasePath = dbPathService.DBPath;
            await TramlineFiveContext.EnsureCreatedAsync();

            await ServiceContainer.ServiceProvider.GetService<HistoryViewModel>().LoadHistoryAsync();
            await ServiceContainer.ServiceProvider.GetService<FavouritesViewModel>().LoadFavouritesAsync();

            StopsLoader.OnStopsUpdated += OnStopsUpdated;
        }

        private void OnStopsUpdated(object sender, EventArgs e)
        {
            ServiceContainer.ServiceProvider.GetService<IApplicationService>().SetStringSetting(Settings.StopsUpdated, DateTime.Now.ToString());
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
