using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TramlineFive.Services;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TramlineFive.Pages
{
    public partial class LocationPromptPage : ContentPage
    {
        private MasterPage masterPage;

        public LocationPromptPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            masterPage = new MasterPage();
        }

        private async void LocationPromptClicked(object sender, EventArgs e)
        {
            PermissionStatus status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted)
                await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

            //IPermissionService permissionService = DependencyService.Get<IPermissionService>();
            //if (!permissionService.HasLocationPermissions())
            //    permissionService.RequestLocationPermissions();

            Application.Current.MainPage = new NavigationPage(masterPage);
        }
    }
}
