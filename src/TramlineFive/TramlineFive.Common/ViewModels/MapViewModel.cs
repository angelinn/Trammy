using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Mapsui;
using Mapsui.Layers;
using Mapsui.Projection;
using Mapsui.Styles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TramlineFive.Common.Models;
using TramlineFive.Common.Services;

namespace TramlineFive.Common.ViewModels
{
    public class MapViewModel : BaseViewModel
    {
        private Map map;
        public ICommand MyLocationCommand { get; private set; }

        public MapViewModel()
        {
            MyLocationCommand = new RelayCommand(async () => await OnMyLocationTappedAsync());
        }

        public void Initialize(Map map)
        {
            this.map = map;
        }

        private bool isMapVisible;
        public bool IsMapVisible
        {
            get
            {
                return isMapVisible;
            }
            set
            {
                isMapVisible = value;
                RaisePropertyChanged();
            }
        }

        private bool isMyLocationVisible;
        public bool IsMyLocationVisible
        {
            get
            {
                return isMyLocationVisible;
            }
            set
            {
                isMyLocationVisible = value;
                RaisePropertyChanged();
            }
        }

        private string myLocationColor = "White";
        public string MyLocationColor
        {
            get
            {
                return myLocationColor;
            }
            set
            {
                myLocationColor = value;
                RaisePropertyChanged();
            }
        }

        public async Task LoadAsync()
        {
            MapService.Initialize(map);

            await LocalizeAsync();
            IsMapVisible = true;
            IsMyLocationVisible = true;
        }

        private async Task OnMyLocationTappedAsync()
        {
            Task t = LocalizeAsync();

            MyLocationColor = "LightGray";
            await Task.Delay(100);
            MyLocationColor = "White";
            await Task.Delay(100);
        }

        private async Task LocalizeAsync()
        {
            await Task.Run(async () =>
            {
                var centerOfSofia = new Mapsui.Geometries.Point(42.6977, 23.3219);
                var sphericalMercatorCoordinate = SphericalMercator.FromLonLat(centerOfSofia.Y, centerOfSofia.X);

                try
                {
                    if (ApplicationService.HasLocationPermissions())
                    {
                        Position position = await ApplicationService.GetCurrentPositionAsync();
                        var c = SphericalMercator.FromLonLat(position.Longitude, position.Latitude);

                        ApplicationService.RunOnUIThread(() =>
                        {
                            map.NavigateTo(c);
                            map.NavigateTo(map.Resolutions[16]);
                        });
                    }
                    else
                    {
                        ApplicationService.RunOnUIThread(() =>
                        {
                            map.NavigateTo(sphericalMercatorCoordinate);
                            map.NavigateTo(map.Resolutions[14]);
                        });
                    }
                }
                catch (Exception ex)
                {
                    ApplicationService.RunOnUIThread(() =>
                    {
                        map.NavigateTo(sphericalMercatorCoordinate);
                        map.NavigateTo(map.Resolutions[14]);
                    });
                }
            });
        }
    }
}
