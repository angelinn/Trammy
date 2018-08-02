using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
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
        private Map map;
        public ICommand SearchCommand { get; private set; }
        public ICommand MyLocationCommand { get; private set; }
        public ICommand OpenHamburgerCommand { get; private set; }
        public ICommand ShowMapCommand { get; private set; }

        public MapViewModel()
        {
            SearchCommand = new RelayCommand(() => ActivateSearch());
            MyLocationCommand = new RelayCommand(async () => await OnMyLocationTappedAsync());
            OpenHamburgerCommand = new RelayCommand(() => MessengerInstance.Send(new SlideHamburgerMessage()));
            ShowMapCommand = new RelayCommand(() => MessengerInstance.Send(new ShowMapMessage()));
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
                            MapService.MoveToUser(userLocationMap);
                        });
                    }
                    else
                    {
                        ApplicationService.RunOnUIThread(() =>
                        {
                            MapService.MoveTo(centerOfSofiaMap);
                        });
                    }
                }
                catch (Exception ex)
                {
                    ApplicationService.RunOnUIThread(() =>
                    {
                        MapService.MoveTo(centerOfSofiaMap);
                    });
                }
            });
        }

        private string query;
        public string Query
        {
            get
            {
                return query;
            }
            set
            {
                query = value;
                RaisePropertyChanged();
            }
        }

        private void ActivateSearch()
        {
            if (!String.IsNullOrEmpty(query))
            {
                MessengerInstance.Send(new StopSelectedMessage(query));
            }
        }
    }
}
