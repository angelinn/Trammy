using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using Mapsui;
using Mapsui.Layers;
using Mapsui.Projections;
using Mapsui.Styles;
using Mapsui.UI;
using SkgtService;
using SkgtService.Models;
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
        public ObservableCollection<ArrivalStopModel> RecommendedStops { get; private set; } = new();
        public List<string> FilteredStops { get; private set; }

        public ICommand MyLocationCommand { get; }
        public ICommand OpenHamburgerCommand { get; }
        public ICommand ShowMapCommand { get; }
        public ICommand ShowSearchCommand { get; }
        public ICommand SearchByCodeCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand SearchFocusedCommand { get; }
        public ICommand SearchUnfocusedCommand { get; }
        public ICommand MapInfoCommand { get; }

        private List<ArrivalStopModel> topFavourites = new();
        private List<ArrivalStopModel> nearbyStops = new();

        private readonly MapService mapService;
        private readonly PublicTransport publicTransport;

        private LocationStatus locationStatus;

        public MapViewModel(MapService mapServiceOther, PublicTransport publicTransport)
        {
            MyLocationCommand = new RelayCommand(async () => await OnMyLocationTappedAsync());
            OpenHamburgerCommand = new RelayCommand(OnOpenHamburger);
            ShowMapCommand = new RelayCommand(OnShowMap);

            SearchByCodeCommand = new RelayCommand<string>(OnSearchByCode);
            SearchCommand = new RelayCommand(() => OnSearchByCode(stopCode));
            SearchFocusedCommand = new RelayCommand(OnSearchFocused);
            SearchUnfocusedCommand = new RelayCommand(OnSearchUnfocused);
            MapInfoCommand = new RelayCommand<MapInfoEventArgs>(OnMapInfo);

            mapService = mapServiceOther;
            this.publicTransport = publicTransport;
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
            MessengerInstance.Register<MapLoadedMessage>(this, async (m) => await CheckForHistory());
        }

        public async Task Initialize(Map nativeMap, Navigator navigator)
        {
            try
            {
                await mapService.Initialize(nativeMap, navigator,
                    ApplicationService.GetStringSetting(Settings.SelectedTileServer, null));
            }
            catch (Exception ex)
            {
                ApplicationService.DisplayToast(ex.Message);
            }

        } 

        public async Task LoadAsync()
        {
            locationStatus = await LocalizeAsync(true);

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

                Position? position = await ApplicationService.GetCurrentPositionAsync();
                if (position != null)
                {
                    ApplicationService.RunOnUIThread(() =>
                    {
                        mapService.MoveToUser(position.Value, true);
                    });

                    MessengerInstance.Send(new UpdateLocationMessage(position.Value));

                    isAnimating = false;
                    HasLocation = true;
                    return LocationStatus.Allowed;
                }

                isAnimating = false;
                HasLocation = false; 
                return LocationStatus.TurnedOff;
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

        private async Task CheckForHistory()
        {
            if (locationStatus != LocationStatus.Allowed)
            {
                HistoryDomain history = await HistoryDomain.GetMostFrequentStopForCurrentHour();
                if (history != null)
                {
                    mapService.MoveAroundStop(history.StopCode);
                }
            }
        }

        private void BuildRecommendedStops()
        {
            RecommendedStops.Clear();

            List<ArrivalStopModel> nearbyWithoutFavourites = nearbyStops.Where(n => !topFavourites.Any(f => f.StopCode == n.StopCode)).ToList();

            //// this causes lag if the stops are added one by one on maui, property raised event lag?
            //List<ArrivalStopModel> newstops = new List<ArrivalStopModel>();
            //if (nearbyWithoutFavourites.Count == 0)
            //{
            //    foreach (ArrivalStopModel stop in topFavourites)
            //        newstops.Add(stop);
            //}
            //else
            //{
            //    if (topFavourites.Count > 0)
            //        newstops.Add(topFavourites[0]);

            //    foreach (ArrivalStopModel stop in nearbyWithoutFavourites.Take(4 - RecommendedStops.Count))
            //        newstops.Add(stop);
            //}

            //RecommendedStops = new ObservableCollection<ArrivalStopModel>(newstops);
            //RaisePropertyChanged(nameof(RecommendedStops));
        }

        private void FilterStops()
        {
            if (String.IsNullOrEmpty(stopCode))
                return;

            if (Char.IsDigit(stopCode[0]))
                FilteredStops = publicTransport.Stops.Where(s => s.Code.Contains(stopCode)).Select(s => s.Code + " " + s.PublicName).Take(5).ToList();
            else
                FilteredStops = publicTransport.Stops.Where(s => s.PublicName.ToLower().Contains(stopCode.ToLower())).Select(s => s.Code + " " + s.PublicName).Take(5).ToList();

            RaisePropertyChanged("FilteredStops");

            SuggestionsHeight = FilteredStops.Count * 50;
        }

        private void OnMapInfo(MapInfoEventArgs e)
        {
            if (isVirtualTablesUp)
            {
                IsVirtualTablesUp = false;
                MessengerInstance.Send(new ShowMapMessage(false));
            }
            else
            {
                mapService.OnMapInfo(e);
            }
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

        private async void OnStopSelectedMessageReceived(StopSelectedMessage message)
        {
            if (message.Clicked)
            {
                IsVirtualTablesUp = true;

                mapService.MoveToStop(message.Selected);
            }
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
