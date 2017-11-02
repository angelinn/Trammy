using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TramlineFive.DataAccess;
using TramlineFive.Services;
using Xamarin.Forms;

namespace TramlineFive
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new Pages.MasterDetail();
        }

        protected override async void OnStart()
        {
            IDatabasePathService dbPathService = DependencyService.Get<IDatabasePathService>();
            TramlineFiveContext.DatabasePath = dbPathService.Path;
            await TramlineFiveContext.EnsureCreatedAsync();
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
