using SkgtService;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TramlineFive.Services;
using Xamarin.Forms;
using Xamarin.Forms.GoogleMaps;
using Xamarin.Forms.Xaml;

namespace TramlineFive.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MapPage : ContentPage
    {
        public MapPage()
        {
            InitializeComponent();

            IPermissionService permissionService = DependencyService.Get<IPermissionService>();
            if (!permissionService.HasLocationPermissions())
                permissionService.RequestLocationPermissions();
            else
                MyMap.MyLocationEnabled = true;

            MyMap.MoveToRegion(MapSpan.FromCenterAndRadius(
                    new Position(42.6977, 23.3219), Distance.FromMiles(0.3)));

            Assembly assembly = typeof(MapPage).GetTypeInfo().Assembly;
            Stream stream = assembly.GetManifestResourceStream("TramlineFive.stops.json");
            foreach (var location in new StopsLoader().LoadStops(stream))
            {
                var position = new Position(location.Lat, location.Lon); // Latitude, Longitude
                var pin = new Pin
                {
                    Type = PinType.Place,
                    Position = position,
                    Label = location.PublicName,
                    Address = location.Code,
                    IsVisible = false
                };
                MyMap.Pins.Add(pin);
            }

            MyMap.CameraIdled += MyMap_CameraIdled;
        }

        private void MyMap_CameraIdled(object sender, CameraIdledEventArgs e)
        {
            if (e.Position.Zoom < 15)
            {
                foreach (Pin pin in MyMap.Pins)
                    pin.IsVisible = false;
            }
            else
            {
                foreach (Pin pin in MyMap.Pins)
                {
                    Position difference = e.Position.Target - pin.Position;
                    pin.IsVisible = (Math.Abs(difference.Latitude) < 0.01 && Math.Abs(difference.Longitude) < 0.01);
                }
            }
        }
    }
}

