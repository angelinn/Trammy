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
        public ICommand ShowSearchCommand { get; private set; }

        private readonly MapService mapService;

        private List<ArrivalStopModel> topFavourites = new List<ArrivalStopModel>();
        private List<ArrivalStopModel> nearbyStops = new List<ArrivalStopModel>();

        private bool hasLocation = true;
        public bool HasLocation
        {
            get
            {
                return hasLocation;
            }
            set
            {
                hasLocation = value;
                RaisePropertyChanged();
            }
        }

        private bool isSearchVisible;
        public bool IsSearchVisible
        {
            get
            {
                return isSearchVisible;
            }
            set
            {
                isSearchVisible = value;
                RaisePropertyChanged();
            }
        }

        public MapViewModel(MapService mapServiceOther)
        {
            MyLocationCommand = new RelayCommand(async () => await OnMyLocationTappedAsync());
            OpenHamburgerCommand = new RelayCommand(() => MessengerInstance.Send(new SlideHamburgerMessage()));
            ShowMapCommand = new RelayCommand(() => MessengerInstance.Send(new ShowMapMessage(false)));
            ShowSearchCommand = new RelayCommand(() => IsSearchVisible = !isSearchVisible);

            mapService = mapServiceOther;
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
                    await mapService.LoadMapAsync(ApplicationService.GetStringSetting(Settings.SelectedTileServer, null));
                }
                else if (m.Name == Settings.MaxPinsZoom && m.Value > 0)
                {
                    mapService.MaxPinsZoom = m.Value;
                    await mapService.LoadMapAsync(ApplicationService.GetStringSetting(Settings.SelectedTileServer, null));
                }
            });

            MessengerInstance.Register<SettingChanged<string>>(this, async (m) => { if (m.Name == Settings.SelectedTileServer) await mapService.LoadMapAsync(m.Value); });
            MessengerInstance.Register<SearchFocusedMessage>(this, m =>
            {
                if (!m.Focused)
                    IsSearchVisible = false;
            });
        }

        public async Task Initialize(Map nativeMap, INavigator navigator)
        {
            await mapService.Initialize(nativeMap, navigator, ApplicationService.GetStringSetting(Settings.SelectedTileServer, null));
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
            if (await LocalizeAsync() == LocationStatus.TurnedOff)
                ApplicationService.OpenLocationUI();
        }

        private async Task<LocationStatus> LocalizeAsync(bool first = false)
        {
            bool isAnimating = true;
            Task _ = Task.Run(async () =>
            {
                while (isAnimating)
                {
                    HasLocation = !HasLocation;
                    await Task.Delay(500);
                }
            });

            try
            {
                if (!await ApplicationService.HasLocationPermissions())
                {
                    if (first)
                    {
                        isAnimating = false;
                        HasLocation = false;

                        return LocationStatus.NoPermissions;
                    }


                    if (!await ApplicationService.RequestLocationPermissions())
                    {
                        isAnimating = false;
                        HasLocation = false;

                        return LocationStatus.NoPermissions;
                    }
                }

                Position position = await ApplicationService.GetCurrentPositionAsync();

                ApplicationService.RunOnUIThread(() =>
                {
                    mapService.MoveToUser(position, true);
                });

                MessengerInstance.Send(new UpdateLocationMessage(position));

                isAnimating = false;
                HasLocation = true;
                return LocationStatus.Allowed;
            }
            catch (Exception ex)
            {
                if (!first)
                    ApplicationService.DisplayToast("Моля включете местоположението.");

                isAnimating = false;
                HasLocation = false;
                return LocationStatus.TurnedOff;
            }
        }

        private void OnFavouritesChanged(FavouritesChangedMessage message)
        {
            topFavourites.Clear();
            topFavourites.AddRange(message.Favourites.Take(4).Select(f => new ArrivalStopModel(f.StopCode, f.Name)));

            BuildRecommendedStops();
        }

        private void OnNearbyStops(NearbyStopsMessage message)
        {
            nearbyStops.Clear();
            nearbyStops.AddRange(message.NearbyStops.Take(4).Select(f => new ArrivalStopModel(f.Code, f.PublicName)));

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

                foreach (ArrivalStopModel stop in nearbyWithoutFavourites.Take(4 - RecommendedStops.Count))
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
