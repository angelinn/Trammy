using GalaSoft.MvvmLight.Command;
using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Projection;
using Mapsui.Providers;
using Mapsui.Styles;
using Mapsui.Utilities;
using SkgtService;
using SkgtService.Models.Locations;
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
        private SymbolStyle pinStyle;
        private List<Feature> features;
        public MapPage()
        {
            InitializeComponent();

            var mapControl = new MapsUIView();
            mapControl.NativeMap.Layers.Add(HumanitarianTileServer.CreateTileLayer());
            mapControl.NativeMap.Info += NativeMap_Info;
            LoadPinStyles();

            var centerOfSofia = new Mapsui.Geometries.Point(42.6977, 23.3219);
            var sphericalMercatorCoordinate = SphericalMercator.FromLonLat(centerOfSofia.Y, centerOfSofia.X);
            mapControl.NativeMap.NavigateTo(sphericalMercatorCoordinate);
            mapControl.NativeMap.NavigateTo(mapControl.NativeMap.Resolutions[14]);
            mapControl.NativeMap.Layers.Add(LoadStops());

            grid.Children.Add(mapControl);
        }

        private void NativeMap_Info(object sender, Mapsui.UI.InfoEventArgs e)
        {
            foreach (var l in features)
            {
                StopLocation location = l["stopObject"] as StopLocation;

                var point = new Mapsui.Geometries.Point(location.Lon, location.Lat);
                var local = SphericalMercator.FromLonLat(point.X, point.Y);
                var difference = e.WorldPosition - local;

                if (Math.Abs(difference.X) < 500 && Math.Abs(difference.Y) < 500)
                {
                    SymbolStyle style = l.Styles.First() as SymbolStyle;
                    style.Enabled = true;
                }
            }
            (sender as Mapsui.Map).ViewChanged(true);
        }

        private void Viewport_ViewportChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(e.PropertyName);
        }

        private ILayer LoadStops()
        {
            Assembly assembly = typeof(MapPage).GetTypeInfo().Assembly;
            Stream stream = assembly.GetManifestResourceStream("TramlineFive.stops.json");

            features = new List<Feature>();

            List<StopLocation> stops = new StopsLoader().LoadStops(stream);
            foreach (var location in stops)
            {
                var centerOfSofia = new Mapsui.Geometries.Point(location.Lon, location.Lat);
                var sphericalMercatorCoordinate = SphericalMercator.FromLonLat(centerOfSofia.X, centerOfSofia.Y);

                Feature feature = new Feature
                {
                    Geometry = sphericalMercatorCoordinate,
                    Styles = new List<IStyle> { new SymbolStyle
                    {
                        Enabled = pinStyle.Enabled,
                        BitmapId = pinStyle.BitmapId
                    }
                    }
                };

                feature["stopObject"] = location;
                features.Add(feature);
            }

            return new Layer
            {
                Name = "Stops layer",
                DataSource = new MemoryProvider(features),
                Style = null
            };
        }

        private void LoadPinStyles()
        {
            Assembly assembly = typeof(MapPage).GetTypeInfo().Assembly;
            Stream stream = assembly.GetManifestResourceStream("TramlineFive.pin.png");
            foreach (string name in assembly.GetManifestResourceNames())
                System.Diagnostics.Debug.WriteLine(name);

            var bitmapId = BitmapRegistry.Instance.Register(stream);

            var centerOfSofia = new Mapsui.Geometries.Point(23.3219, 42.6977);
            var sphericalMercatorCoordinate = SphericalMercator.FromLonLat(centerOfSofia.X, centerOfSofia.Y);
            // LoadStops();
            pinStyle = new SymbolStyle
            {
                BitmapId = bitmapId,
                Enabled = false
            };
        }

    }
}

