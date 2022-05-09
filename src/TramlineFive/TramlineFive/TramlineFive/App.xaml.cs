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

            var dict = Current.Resources.MergedDictionaries;

            //IPermissionService permissionService = DependencyService.Get<IPermissionService>();

            //if (!permissionService.HasLocationPermissions())
            //    MainPage = new Pages.LocationPromptPage();
            //else
            //    MainPage = new NavigationPage(new MasterPage()); 

            string theme = Preferences.Get("Theme", "Light");

            Messenger.Default.Register<ChangeThemeMessage>(this, m =>
            {
                ResourceDictionary themeDictionary = Current.Resources.MergedDictionaries.FirstOrDefault(d => d.ContainsKey("PrimaryColor"));
                if (themeDictionary != null)
                    Current.Resources.MergedDictionaries.Remove(themeDictionary);

                if (m.Name == "Light")
                {
                    Current.UserAppTheme = OSAppTheme.Light;
                    Current.Resources.MergedDictionaries.Add(new LightTheme());
                }
                else
                {
                    Current.UserAppTheme = OSAppTheme.Dark;
                    Current.Resources.MergedDictionaries.Add(new DarkTheme());
                }
            });

            Messenger.Default.Send(new ChangeThemeMessage(theme));
            DependencyService.Get<IVersionCheckingService>().CreateTask();
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
