using Mapsui.Projection;
using Mapsui.Utilities;
using SkgtService;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TramlineFive.Common.Maps;
using TramlineFive.Services;
using TramlineFive.Views;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TramlineFive.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MapPage : ContentPage
    {
        public MapPage()
        {
            InitializeComponent();

            var mapControl = new MapsUIView();
            mapControl.NativeMap.Layers.Add(HumanitarianTileServer.CreateTileLayer());
            var centerOfSofia = new Point(42.6977, 23.3219);
            var sphericalMercatorCoordinate = SphericalMercator.FromLonLat(centerOfSofia.Y, centerOfSofia.X);
            mapControl.NativeMap.NavigateTo(sphericalMercatorCoordinate);
            mapControl.NativeMap.NavigateTo(mapControl.NativeMap.Resolutions[14]);

            grid.Children.Add(mapControl);
        }

        protected override async void OnAppearing()
        {
            //if (initialized)
            //    return;

            //initialized = true;

            //await Task.Run(() =>
            //{
            //    MyMap = new Map();
            //    MyMap.MapType = MapType.Street;

            //    MyMap.MoveToRegion(MapSpan.FromCenterAndRadius(
            //new Position(42.6977, 23.3219), Distance.FromMiles(0.3)));

            //    Assembly assembly = typeof(MapPage).GetTypeInfo().Assembly;
            //    Stream stream = assembly.GetManifestResourceStream("TramlineFive.stops.json");
            //    foreach (var location in new StopsLoader().LoadStops(stream))
            //    {
            //        var position = new Position(location.Lat, location.Lon); // Latitude, Longitude
            //        var pin = new Pin
            //        {
            //            Type = PinType.Place,
            //            Position = position,
            //            Label = location.PublicName,
            //            Address = location.Code,
            //            IsVisible = false
            //        };
            //        MyMap.Pins.Add(pin);
            //    }

            //    IPermissionService permissionService = DependencyService.Get<IPermissionService>();

            //    if (permissionService.HasLocationPermissions())
            //        MyMap.MyLocationEnabled = true;

            //    MyMap.CameraIdled += MyMap_CameraIdled;
            //    Device.BeginInvokeOnMainThread(() =>
            //    {
            //        grid.Children.RemoveAt(0);
            //        grid.Children.Add(MyMap);
            //    });
            //});
        }

        //private void MyMap_CameraIdled(object sender, CameraIdledEventArgs e)
        //{
        //    if (e.Position.Zoom < 15)
        //    {
        //        foreach (Pin pin in MyMap.Pins)
        //            pin.IsVisible = false;
        //    }
        //    else
        //    {
        //        foreach (Pin pin in MyMap.Pins)
        //        {
        //            Position difference = e.Position.Target - pin.Position;
        //            pin.IsVisible = (Math.Abs(difference.Latitude) < 0.01 && Math.Abs(difference.Longitude) < 0.01);
        //        }
        //    }
        //}
    }
}

