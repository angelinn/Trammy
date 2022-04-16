using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using Mapsui;
using Mapsui.Layers;
using Mapsui.Projections;
using Mapsui.Styles;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TramlineFive.Common.Messages;
using TramlineFive.Common.Models;
using TramlineFive.Common.Services;
using TramlineFive.DataAccess.Domain;

namespace TramlineFive.Common.ViewModels
{
    public class MapViewModel : BaseViewModel
    {
        public ObservableCollection<ArrivalStopModel> RecommendedStops { get; private set; } = new ObservableCollection<ArrivalStopModel>();

        public ICommand MyLocationCommand { get; private set; }
        public ICommand OpenHamburgerCommand { get; private set; }
        public ICommand ShowMapCommand { get; private set; }

        private MapService mapService;

        private List<ArrivalStopModel> topFavourites = new List<ArrivalStopModel>();
        private List<ArrivalStopModel> nearbyStops = new List<ArrivalStopModel>();

        public MapViewModel()
        {
            MyLocationCommand = new RelayCommand(async () => await OnMyLocationTappedAsync());
            OpenHamburgerCommand = new RelayCommand(() => MessengerInstance.Send(new SlideHamburgerMessage()));
            ShowMapCommand = new RelayCommand(() => MessengerInstance.Send(new ShowMapMessage(false)));

            mapService = SimpleIoc.Default.GetInstance<MapService>();
            int maxTextZoom = ApplicationService.GetIntSetting(Settings.MaxTextZoom, 0);
            int maxPinsZoom = ApplicationService.GetIntSetting(Settings.MaxPinsZoom, 0);

            if (maxTextZoom > 0)
                mapService.MaxTextZoom = maxTextZoom;
            if (maxPinsZoom > 0)
                mapService.MaxPinsZoom = maxPinsZoom;

            MessengerInstance.Register<StopSelectedMessage>(this, OnStopSelectedMessageReceived);
            MessengerInstance.Register<FavouritesChangedMessage>(this, OnFavouritesChanged);
            MessengerInstance.Register<NearbyStopsMessage>(this, OnNearbyStops);
            MessengerInstance.Register<SettingChanged<int>>(this, async (m) =>
            {
                if (m.Name == Settings.MaxTextZoom && m.Value > 0)
                {
                    mapService.MaxTextZoom = m.Value;
                    await mapService.ReloadMapAsync();
                }
                else if (m.Name == Settings.MaxPinsZoom && m.Value > 0)
                {
                    mapService.MaxPinsZoom = m.Value;
                    await mapService.ReloadMapAsync();
                }
            });
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
            await LocalizeAsync(true);
            IsMapVisible = true;
            IsMyLocationVisible = true;
        }

        private async Task OnMyLocationTappedAsync()
        {
            Task<bool> t = LocalizeAsync();

            MyLocationColor = "LightGray";
            await Task.Delay(100);
            MyLocationColor = "White";
            await Task.Delay(100);
        }

        private async Task<bool> LocalizeAsync(bool first = false)
        {
            return await Task.Run(async () =>
            {
                try
                {
                    if (ApplicationService.HasLocationPermissions())
                    {
                        Position position = await ApplicationService.GetCurrentPositionAsync();

                        ApplicationService.RunOnUIThread(() =>
                        {
                            mapService.MoveToUser(position, true);
                        });

                        MessengerInstance.Send(new UpdateLocationMessage(position));

                        return true;
                    }

                    ApplicationService.DisplayToast("Достъпът до местоположение не е позволен");

                    return false;

                }
                catch (Exception ex)
                {
                    if (!first)
                        ApplicationService.DisplayToast("Моля включете местоположението.");

                    return false;
                }
            });
        }

        private void OnFavouritesChanged(FavouritesChangedMessage message)
        {
            topFavourites.Clear();
            topFavourites.AddRange(message.Favourites.Take(5).Select(f => new ArrivalStopModel(f.StopCode, f.Name)));

            BuildRecommendedStops();
        }

        private void OnNearbyStops(NearbyStopsMessage message)
        {
            nearbyStops.Clear();
            nearbyStops.AddRange(message.NearbyStops.Take(5).Select(f => new ArrivalStopModel(f.Code, f.PublicName)));

            BuildRecommendedStops();
        }

        private void BuildRecommendedStops()
        {
            RecommendedStops.Clear();

            List<ArrivalStopModel> nearbyWithoutFavourites = nearbyStops.Where(n => !topFavourites.Any(f => f.StopCode == n.StopCode)).ToList();

            if (nearbyWithoutFavourites.Count == 0)
            {
                foreach (ArrivalStopModel stop in topFavourites)
                    RecommendedStops.Add(stop);
            }
            else
            {
                if (topFavourites.Count > 0)
                    RecommendedStops.Add(topFavourites[0]);

                foreach (ArrivalStopModel stop in nearbyWithoutFavourites)
                    RecommendedStops.Add(stop);
            }
        }

        private void OnStopSelectedMessageReceived(StopSelectedMessage message)
        {
            if (message.Clicked)
                mapService.MoveToStop(message.Selected);
        }
    }
}
