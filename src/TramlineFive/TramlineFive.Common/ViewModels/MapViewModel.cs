using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Mapsui;
using Mapsui.Layers;
using Mapsui.Projections;
using Mapsui.Styles;
using Mapsui.UI;
using Sentry;
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
using TramlineFive.Common.Services.Maps;
using TramlineFive.DataAccess.Domain;

namespace TramlineFive.Common.ViewModels
{
    public partial class MapViewModel : BaseViewModel
    {
        public ObservableCollection<ArrivalStopModel> RecommendedStops { get; private set; } = new();
        public List<string> FilteredStops { get; private set; }

        public ICommand SearchCommand { get; }

        private List<ArrivalStopModel> topFavourites = new();
        private List<ArrivalStopModel> nearbyStops = new();

        private readonly MapService mapService;
        private readonly PublicTransport publicTransport;

        private LocationStatus locationStatus;

        public MapViewModel(MapService mapServiceOther, PublicTransport publicTransport)
        {
            SearchCommand = new CommunityToolkit.Mvvm.Input.RelayCommand(() => SearchByCode(stopCode));

            mapService = mapServiceOther;
            this.publicTransport = publicTransport;
            int maxTextZoom = ApplicationService.GetIntSetting(Settings.MaxTextZoom, 0);
            int maxPinsZoom = ApplicationService.GetIntSetting(Settings.MaxPinsZoom, 0);

            if (maxTextZoom > 0)
                mapService.MaxTextZoom = maxTextZoom;
            if (maxPinsZoom > 0)
                mapService.MaxPinsZoom = maxPinsZoom;

            Messenger.Register<StopSelectedMessage>(this, (r, m) => OnStopSelectedMessageReceived(m));
            Messenger.Register<FavouritesChangedMessage>(this, (r, m) => OnFavouritesChanged(m));
            Messenger.Register<NearbyStopsMessage>(this, (r, m) => OnNearbyStops(m));
            Messenger.Register<SettingChanged<int>>(this, async (r, m) => await OnIntSettingChangedAsync(m));
            Messenger.Register<SettingChanged<string>>(this, async (r, m) => await OnStringSettingChangedAsync(m));
            Messenger.Register<MapLoadedMessage>(this, async (r, m) => await CheckForHistory());
        }

        public async Task Initialize(Map nativeMap)
        {
            try
            {
                await mapService.Initialize(nativeMap);
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
                ApplicationService.DisplayToast(ex.Message);
            }

        }

        [RelayCommand]
        private async Task MyLocation()
        {
            if (await LocalizeAsync() == LocationStatus.TurnedOff)
                ApplicationService.OpenLocationUI();
        }

        public async Task<LocationStatus> LocalizeAsync(bool first = false)
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

                        locationStatus = LocationStatus.NoPermissions;
                        return locationStatus;
                    }


                    if (!await ApplicationService.RequestLocationPermissions())
                    {
                        isAnimating = false;
                        HasLocation = false;

                        locationStatus = LocationStatus.NoPermissions;
                        return locationStatus;
                    }
                }

                Position? position = await ApplicationService.GetCurrentPositionAsync();
                if (position != null)
                {
                    ApplicationService.RunOnUIThread(() =>
                    {
                        mapService.MoveToUser(position.Value, true);
                    });

                    Messenger.Send(new UpdateLocationMessage(position.Value));

                    isAnimating = false;
                    HasLocation = true;


                    locationStatus = LocationStatus.Allowed;
                    return locationStatus;
                }

                isAnimating = false;
                HasLocation = false;
                locationStatus = LocationStatus.TurnedOff;
                return locationStatus;
            }
            catch (Exception ex)
            {
                if (!first)
                    ApplicationService.DisplayToast("Моля включете местоположението.");

                isAnimating = false;
                HasLocation = false;

                locationStatus = LocationStatus.TurnedOff;
                return locationStatus;
            }
        }

        private void OnFavouritesChanged(FavouritesChangedMessage message)
        {
            topFavourites.Clear();
            topFavourites.AddRange(message.Favourites.Take(4).Select(f => new ArrivalStopModel(f.StopCode, f.Name)));

            //BuildRecommendedStops();
        }

        private void OnNearbyStops(NearbyStopsMessage message)
        {
            nearbyStops.Clear();
            nearbyStops.AddRange(message.NearbyStops.Take(4).Select(f => new ArrivalStopModel(f.Code, f.PublicName)));

            //BuildRecommendedStops();
        }

        private async Task CheckForHistory()
        {
            if (locationStatus != LocationStatus.Allowed)
            {
                HistoryDomain history = await HistoryDomain.GetMostFrequentStopForCurrentHour();
                if (history != null)
                {
                    mapService.MoveAroundStop(history.StopCode, true);
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
            if (String.IsNullOrEmpty(StopCode))
                return;

            if (Char.IsDigit(StopCode[0]))
                FilteredStops = publicTransport.Stops.Where(s => s.Code.Contains(StopCode)).Select(s => s.Code + " " + s.PublicName).Take(5).ToList();
            else
                FilteredStops = publicTransport.Stops.Where(s => s.PublicName.ToLower().Contains(StopCode.ToLower())).Select(s => s.Code + " " + s.PublicName).Take(5).ToList();

            OnPropertyChanged(nameof(FilteredStops));

            SuggestionsHeight = FilteredStops.Count * 50;
        }

        [RelayCommand]
        private void MapInfo(MapInfoEventArgs e)
        {
            if (IsVirtualTablesUp)
            {
                IsVirtualTablesUp = false;
            }

            mapService.OnMapInfo(e);
        }

        private async Task OnIntSettingChangedAsync(SettingChanged<int> m)
        {
            if (m.Name == Settings.MaxTextZoom && m.Value > 0)
            {
                mapService.MaxTextZoom = m.Value;
                await mapService.SetupMapAsync();
            }
            else if (m.Name == Settings.MaxPinsZoom && m.Value > 0)
            {
                mapService.MaxPinsZoom = m.Value;
                await mapService.SetupMapAsync();
            }
        }

        private async Task OnStringSettingChangedAsync(SettingChanged<string> m)
        {
            if (m.Name == Settings.SelectedTileServer)
                mapService.ChangeTileServer(m.Value, null, null);
            else if (m.Name == Settings.FetchingStrategy)
                mapService.ChangeTileServer(null, m.Value, null);
            else if (m.Name == Settings.RenderStrategy)
                mapService.ChangeTileServer(null, null, m.Value);
        }

        private void OnStopSelectedMessageReceived(StopSelectedMessage message)
        {
            if (message.Value.Clicked)
            {
                IsVirtualTablesUp = true;

                mapService.MoveToStop(message.Value.Selected);
            }
        }

        [RelayCommand]
        private void OnSearchFocused()
        {
            IsFocused = true;
        }

        [RelayCommand]
        private void OnSearchUnfocused()
        {
            IsFocused = false;
            IsSearching = false;
        }

        [RelayCommand]
        private void SearchByCode(string i)
        {
            IsVirtualTablesUp = true;
            Messenger.Send(new StopSelectedMessage(new StopSelectedMessagePayload(i, true)));
        }

        [RelayCommand]
        private void ShowMap()
        {
            IsVirtualTablesUp = false;
            Messenger.Send(new ShowMapMessage(false));
        }

        [RelayCommand]
        private void OpenHamburger()
        {
            Messenger.Send(new SlideHamburgerMessage());
        }

        [ObservableProperty]
        private string selectedSuggestion;

        partial void OnSelectedSuggestionChanged(string value)
        {
            if (!String.IsNullOrEmpty(value))
            {
                string code = value.Substring(0, 4);
                Messenger.Send(new StopSelectedMessage(new StopSelectedMessagePayload(code, true)));
            }
        }

        [ObservableProperty]
        private int suggestionsHeight;

        [ObservableProperty]
        private bool hasLocation = true;

        [ObservableProperty]
        private bool isVirtualTablesUp;

        [ObservableProperty]
        private bool isSearching;

        [ObservableProperty]
        private bool isFocused;

        [ObservableProperty]
        private string stopCode;

        partial void OnStopCodeChanged(string value)
        {
            if (IsFocused && !String.IsNullOrEmpty(value))
            {
                IsSearching = true;
                FilterStops();
            }
        }
    }
}
