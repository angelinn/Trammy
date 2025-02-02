using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TramlineFive.Maui;
using TramlineFive.Services;
using Microsoft.Maui.ApplicationModel;

namespace TramlineFive.Pages
{
    public partial class LocationPromptPage : ContentPage
    {
        public LocationPromptPage()
        {
            InitializeComponent();
        }

        private async void LocationPromptClicked(object sender, EventArgs e)
        {
            PermissionStatus status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted)
                await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

            //IPermissionService permissionService = DependencyService.Get<IPermissionService>();
            //if (!permissionService.HasLocationPermissions())
            //    permissionService.RequestLocationPermissions();

            await Shell.Current.GoToAsync("//Main");
        }
    }
}
