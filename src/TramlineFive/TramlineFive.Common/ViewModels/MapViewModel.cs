using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using Mapsui;
using Mapsui.Layers;
using Mapsui.Projections;
using Mapsui.Styles;
using Mapsui.UI;
using SkgtService;
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
        public ObservableCollection<ArrivalStopModel> RecommendedStops { get; } = new();
        public List<string> FilteredStops { get; private set; }

        public ICommand MyLocationCommand { get; }
        public ICommand OpenHamburgerCommand { get; }
        public ICommand ShowMapCommand { get; }
        public ICommand ShowSearchCommand { get; }
        public ICommand SearchByCodeCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand SearchFocusedCommand { get; }
        public ICommand SearchUnfocusedCommand { get; }

        private readonly MapService mapService;

        private List<ArrivalStopModel> topFavourites = new List<ArrivalStopModel>();
        private List<ArrivalStopModel> nearbyStops = new List<ArrivalStopModel>();

        public MapViewModel(MapService mapServiceOther)
        {
            MyLocationCommand = new RelayCommand(async () => await OnMyLocationTappedAsync());
            OpenHamburgerCommand = new RelayCommand(OnOpenHamburger);
            ShowMapCommand = new RelayCommand(OnShowMap);

            SearchByCodeCommand = new RelayCommand<string>(OnSearchByCode);
            SearchCommand = new RelayCommand(() => OnSearchByCode(stopCode));
            SearchFocusedCommand = new RelayCommand(OnSearchFocused);
            SearchUnfocusedCommand = new RelayCommand(OnSearchUnfocused);

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
            MessengerInstance.Register<SettingChanged<int>>(this, async (m) => await OnIntSettingChangedAsync(m));
            MessengerInstance.Register<SettingChanged<string>>(this, async (m) => await OnStringSettingChangedAsync(m));
        }

        public async Task Initialize(Map nativeMap, INavigator navigator)
        {
            await mapService.Initialize(nativeMap, navigator, ApplicationService.GetStringSetting(Settings.SelectedTileServer, null));
        }

        public void OnMapInfo(MapInfoEventArgs e)
        {
            if (isVirtualTablesUp)
            {
                IsVirtualTablesUp = false;
                MessengerInstance.Send(new ShowMapMessage(false));
            }
            else
                mapService.OnMapInfo(e);
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

        private void FilterStops()
        {
            if (String.IsNullOrEmpty(stopCode))
                return;

            if (Char.IsDigit(stopCode[0]))
                FilteredStops = StopsLoader.Stops.Where(s => s.Code.Contains(stopCode)).Select(s => s.Code + " " + s.PublicName).Take(5).ToList();
            else
                FilteredStops = StopsLoader.Stops.Where(s => s.PublicName.ToLower().Contains(stopCode.ToLower())).Select(s => s.Code + " " + s.PublicName).Take(5).ToList();

            RaisePropertyChanged("FilteredStops");

            SuggestionsHeight = FilteredStops.Count * 50;
        }

        private async Task OnIntSettingChangedAsync(SettingChanged<int> m)
        {
            if (m.Name == Settings.MaxTextZoom && m.Value > 0)
            {
                mapService.MaxTextZoom = m.Value;
                await mapService.SetupMapAsync(ApplicationService.GetStringSetting(Settings.SelectedTileServer, null));
            }
            else if (m.Name == Settings.MaxPinsZoom && m.Value > 0)
            {
                mapService.MaxPinsZoom = m.Value;
                await mapService.SetupMapAsync(ApplicationService.GetStringSetting(Settings.SelectedTileServer, null));
            }
        }

        private async Task OnStringSettingChangedAsync(SettingChanged<string> m)
        {
            if (m.Name == Settings.SelectedTileServer)
                await mapService.SetupMapAsync(m.Value);
        }

        private void OnStopSelectedMessageReceived(StopSelectedMessage message)
        {
            if (message.Clicked)
                mapService.MoveToStop(message.Selected);
        }

        private void OnSearchFocused()
        {
            IsFocused = true;
        }

        private void OnSearchUnfocused()
        {
            IsFocused = false;
            IsSearching = false;
        }

        private void OnSearchByCode(string i)
        {
            IsVirtualTablesUp = true;
            MessengerInstance.Send(new StopSelectedMessage(i, true));
        }

        private void OnShowMap()
        {
            IsVirtualTablesUp = false;
            MessengerInstance.Send(new ShowMapMessage(false));
        }

        private void OnOpenHamburger()
        {
            MessengerInstance.Send(new SlideHamburgerMessage());
        }

        private string selectedSuggestion;
        public string SelectedSuggestion
        {
            get
            {
                return selectedSuggestion;
            }
            set
            {
                selectedSuggestion = value;

                if (!String.IsNullOrEmpty(selectedSuggestion))
                {
                    string code = selectedSuggestion.Substring(0, 4);
                    MessengerInstance.Send(new StopSelectedMessage(code, true));
                }

                RaisePropertyChanged();
            }
        }

        private int suggestionsHeight;
        public int SuggestionsHeight
        {
            get
            {
                return suggestionsHeight;
            }
            set
            {
                suggestionsHeight = value;
                RaisePropertyChanged();
            }
        }


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

        private bool isVirtualTablesUp;
        public bool IsVirtualTablesUp
        {
            get
            {
                return isVirtualTablesUp;
            }
            set
            {
                isVirtualTablesUp = value;
                RaisePropertyChanged();
            }
        }

        private bool isSearching;
        public bool IsSearching
        {
            get
            {
                return isSearching;
            }
            set
            {
                isSearching = value;
                RaisePropertyChanged();
            }
        }

        private bool isFocused;
        public bool IsFocused
        {
            get
            {
                return isFocused;
            }
            set
            {
                isFocused = value;
                RaisePropertyChanged();
            }
        }

        private string stopCode;
        public string StopCode
        {
            get
            {
                return stopCode;
            }
            set
            {
                stopCode = value;
                if (isFocused && !String.IsNullOrEmpty(stopCode))
                {
                    IsSearching = true;
                    FilterStops();
                }

                RaisePropertyChanged();
            }
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

    }
}
