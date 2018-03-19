using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Projection;
using Mapsui.Providers;
using Mapsui.Styles;
using Mapsui.Utilities;
using Plugin.Geolocator;
using Plugin.Geolocator.Abstractions;
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
using TramlineFive.Common.Messages;
using TramlineFive.Common.Services;
using TramlineFive.Common.ViewModels;
using TramlineFive.Services;
using TramlineFive.Views;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TramlineFive.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MapPage : Grid
    {
        private bool initialized;

        public MapPage()
        {
            InitializeComponent();
        }

        public async Task OnAppearing()
        {
            if (initialized)
                return;

            initialized = true;
            await (BindingContext as MapViewModel).LoadAsync();
        }
    }
}
