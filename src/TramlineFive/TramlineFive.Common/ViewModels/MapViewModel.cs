using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using Mapsui;
using Mapsui.Geometries;
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
using TramlineFive.Common.Messages;
using TramlineFive.Common.Models;
using TramlineFive.Common.Services;

namespace TramlineFive.Common.ViewModels
{
    public class MapViewModel : BaseViewModel
    {
        public ICommand MyLocationCommand { get; private set; }
        public ICommand OpenHamburgerCommand { get; private set; }
        public ICommand ShowMapCommand { get; private set; }

        private MapService mapService;

        public MapViewModel()
        {
            MyLocationCommand = new RelayCommand(async () => await OnMyLocationTappedAsync());
            OpenHamburgerCommand = new RelayCommand(() => MessengerInstance.Send(new SlideHamburgerMessage()));
            ShowMapCommand = new RelayCommand(() => MessengerInstance.Send(new ShowMapMessage(false)));

            mapService = SimpleIoc.Default.GetInstance<MapService>();

            MessengerInstance.Register<StopSelectedMessage>(this, (m) => mapService.MoveToStop(m.Selected));
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
                Point centerOfSofia = new Point(42.6977, 23.3219);
                Point centerOfSofiaMap = SphericalMercator.FromLonLat(centerOfSofia.Y, centerOfSofia.X);

                try
                {
                    if (ApplicationService.HasLocationPermissions())
                    {
                        Position position = await ApplicationService.GetCurrentPositionAsync();
                        Point userLocationMap = SphericalMercator.FromLonLat(position.Longitude, position.Latitude);

                        ApplicationService.RunOnUIThread(() =>
                        {
                            mapService.MoveToUser(userLocationMap);
                        });
                    }
                    else
                    {
                        ApplicationService.RunOnUIThread(() =>
                        {
                            mapService.MoveTo(centerOfSofiaMap, 14);
                        });
                    }
                }
                catch (Exception ex)
                {
                    ApplicationService.RunOnUIThread(() =>
                    {
                        mapService.MoveTo(centerOfSofiaMap, 14);
                    });
                }
            });
        }
    }
}
