using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Mapsui;
using Sentry;
using SkgtService;
using SkgtService.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using TramlineFive.Common.Messages;
using TramlineFive.Common.Models;
using TramlineFive.Common.Services.Maps;
using TramlineFive.DataAccess.Domain;

namespace TramlineFive.Common.ViewModels;

public partial class MapViewModel : BaseViewModel
{
    public enum TouchType
    {
        None,
        Pressed,
        Released,
        Moved
    }

    public enum SheetState
    {
        None,
        Peek = 1,
        Medium = 2,
        Large = 4
    }

    public List<string> FilteredStops { get; private set; }

    public double OverlayHeightInPixels
    {
        get => mapService.OverlayHeightInPixels;
        set => mapService.OverlayHeightInPixels = value;
    }

    private readonly MapService mapService;
    private readonly PublicTransport publicTransport;

    private LocationStatus locationStatus;
    private readonly string dbPath;

    public MapViewModel(MapService mapServiceOther, PublicTransport publicTransport, StopsConfigurator configurator)
    {
        mapService = mapServiceOther;
        this.publicTransport = publicTransport;
        int maxTextZoom = ApplicationService.GetIntSetting(Settings.MaxTextZoom, 0);
        int maxPinsZoom = ApplicationService.GetIntSetting(Settings.MaxPinsZoom, 0);

        dbPath = configurator.DatabasePath;

        if (maxTextZoom > 0)
            mapService.MaxTextZoom = maxTextZoom;
        if (maxPinsZoom > 0)
            mapService.MaxPinsZoom = maxPinsZoom;

        Messenger.Register<StopSelectedMessage>(this, (r, m) => OnStopSelectedMessageReceived(m));
        Messenger.Register<SettingChanged<int>>(this, async (r, m) => await OnIntSettingChangedAsync(m));
        Messenger.Register<SettingChanged<string>>(this, (r, m) => OnStringSettingChanged(m));
        Messenger.Register<MapLoadedMessage>(this, async (r, m) => await MoveToMostFrequentStopForCurrentHour());
        Messenger.Register<ShowRouteMessage>(this, (r, m) => IsVirtualTablesUp = false);
    }

    public async Task SetupFullMapAsync()
    {
        try
        {
            await mapService.SetupMapAsync();
        }
        catch (Exception ex)
        {
            SentrySdk.CaptureException(ex);
            ApplicationService.DisplayToast(ex.Message);
        }

    }
    public void LoadInitialMap(Map map, string tileServer, string dataFetchStrategy, string renderFetchStrategy, double x, double y)
    {
        mapService.LoadInitialMap(map, tileServer, dataFetchStrategy, renderFetchStrategy, dbPath, x, y);
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
        _ = Task.Run(async () =>
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

            bool success = true;

            if (locationStatus == LocationStatus.Allowed)
            {
                mapService.FollowUser();
            }
            else
            {
                success = await ApplicationService.SubscribeForLocationChangeAsync((p) =>
                {
                    Messenger.Send(new UpdateLocationMessage(p));
                    mapService.FollowUser();
                    mapService.MoveTo(p);
                });
            }
            isAnimating = false;

            if (success)
            {
                locationStatus = LocationStatus.Allowed;
                HasLocation = true;
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
                ApplicationService.DisplayToast($"{ex.Message} Моля включете местоположението.");

            isAnimating = false;
            HasLocation = false;

            locationStatus = LocationStatus.TurnedOff;
            return locationStatus;
        }
    }

    private async Task MoveToMostFrequentStopForCurrentHour()
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

    private void OnStringSettingChanged(SettingChanged<string> m)
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
        IsVirtualTablesUp = true;
        mapService.MoveToStop(message.Selected);
    }

    public bool NavigateBack()
    {
        if (IsVirtualTablesUp)
        {
            IsVirtualTablesUp = false;
            _ = mapService.ShowNearbyStops();

            return true;
        }

        return mapService.HandleGoBack();
    }

    private bool hasMoved;
    public async Task OnMapTouchAsync(TouchType touch)
    {
        mapService.StopFollowing();
       
        if (touch == TouchType.Released)
        {
            if (hasMoved)
            {
                await mapService.ShowNearbyStops();
                Debug.WriteLine($"Show stops");

                hasMoved = false;
            }
        }
        else if (touch == TouchType.Moved)
        {
            if (!hasMoved)
                hasMoved = true;
        }
        else if (touch == TouchType.Pressed)
        {
            if (IsVirtualTablesUp)
            {
                IsVirtualTablesUp = false;
                _ = mapService.ShowNearbyStops();
                //Messenger.Send(new HideVirtualTablesMessage());
            }
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
    private void Search()
    {
        SearchByCode(StopCode);
    }

    [RelayCommand]
    private void SearchByCode(string i)
    {
        IsVirtualTablesUp = true;
        Messenger.Send(new StopSelectedMessage(i));
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
            Messenger.Send(new StopSelectedMessage(code));
        }
    }

    [ObservableProperty]
    private int suggestionsHeight;

    [ObservableProperty]
    private bool hasLocation = true;

    [ObservableProperty]
    private bool isVirtualTablesUp;
    partial void OnIsVirtualTablesUpChanged(bool value)
    {
        if (!value)
        {
            CurrentVirtualTablesState = SheetState.Medium; 
            ApplicationService.ResetStatusBarStyle();

            _ = mapService.ShowNearbyStops();
        }
    }

    [ObservableProperty]
    private SheetState currentVirtualTablesState = SheetState.Medium;
    partial void OnCurrentVirtualTablesStateChanged(SheetState value)
    {
        Messenger.Send(new HeightChangedMessage(value));
    }

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
