using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ExCSS;
using Mapsui.Utilities;
using SkgtService.Models;
using TramlineFive.Common.GTFS;
using TramlineFive.Common.Services.Maps;
using TramlineFive.DataAccess;
using TramlineFive.DataAccess.Entities.GTFS;

namespace TramlineFive.Common.ViewModels;

public class ArrivalVM
{
    public string ArrivalDisplay { get; set; } // e.g. "5 min"
    public bool MinutesDisplay { get; set; }
    public string DepartureTime { get; set; }
    public DateTime DepartureDateTime { get; set; }
    public bool Realtime { get; set; }
    public string Headsign { get; set; }
    public string TripId { get; set; }
    public int Delay { get; set; }
}

public partial class RouteDetailViewModel : BaseViewModel
{
    [ObservableProperty]
    private string stopCode;

    [ObservableProperty]
    private string stopName;

    [ObservableProperty]
    private string lineName;

    [ObservableProperty]
    private TransportType vehicleType;

    public ObservableRangeCollection<ArrivalVM> ScheduledArrivals { get; set; } = new();

    [ObservableProperty]
    private bool isLoading;

    private const int PAGE_SIZE = 20;
    private int currentPage = 0;
    private List<ArrivalVM> allArrivals = new();

    private readonly GTFSClient GtfsClient;
    public RouteDetailViewModel(GTFSClient gtfsClient)
    { 
        GtfsClient = gtfsClient;
    }

    public static bool TryNormalize(string gtfsTime, out DateTime result)
    {
        result = default;

        DateTime now = DateTime.Now;

        var parts = gtfsTime.Split(':');
        if (parts.Length != 3) return false;

        int h = int.Parse(parts[0]);
        int m = int.Parse(parts[1]);
        int s = int.Parse(parts[2]);

        // GTFS: hours may exceed 24 → next calendar day(s)
        int dayOffset = h / 24;
        int normalizedHour = h % 24;

        // build the intended datetime
        DateTime departure = now.Date.AddHours(normalizedHour)
                                     .AddMinutes(m)
                                     .AddSeconds(s)
                                     .AddDays(dayOffset);

        // ---------- NIGHT LOGIC FIX ----------
        // if GTFS hour < 24 but departure time is less than "now" AND it's after midnight,
        // we DO NOT treat it as next day: it's earlier today (still correct)
        if (dayOffset > 0 && now.Hour < 3 && departure.Hour < 3)
        {
            // both are after midnight → do nothing
            result = departure.AddDays(-dayOffset);
            return true;
        }

        result = departure;
        return true;
    }

    private async Task LoadScheduledArrivalsAsync(string routeId)
    {
        List<(Trip, StopTime)> departures = await GTFSContext.GetNextDeparturesForRouteAtStopAsync(routeId, StopCode, DateTime.Now, 10);

        foreach ((Trip trip, StopTime stopTime) in departures)
        {
            TryNormalize(stopTime.DepartureTime, out DateTime departureTime);
      
            int minutes = (int)(departureTime - DateTime.Now).TotalMinutes;

            var arrival = allArrivals.FirstOrDefault(a => a.TripId == trip.TripId);
            if (arrival != null)
            {
                arrival.Delay = (int)(arrival.DepartureDateTime - departureTime).TotalMinutes;
            }
            else
            {
                allArrivals.Add(new ArrivalVM
                {
                    ArrivalDisplay = minutes > 100 ? departureTime.ToString("HH:mm") : minutes.ToString(),
                    MinutesDisplay = minutes <= 100,
                    DepartureTime = departureTime.Date == DateTime.Today ? departureTime.ToString("HH:mm") : departureTime.ToString("dd MMM HH:mm"),
                    Realtime = false,
                    Headsign = trip.TripHeadsign
                });
            }

            await Task.Yield();
        }


        IsLoading = false;
    }

    public async Task LoadAsync(RouteArrivalInformation arrival, string stopCode, string stopName)
    {
        ScheduledArrivals.Clear();
        currentPage = 0;

        IsLoading = true;

        LineName = arrival.LineName;
        StopCode = stopCode;
        StopName = stopName;
        VehicleType = arrival.VehicleType;

        await Task.Yield();

        allArrivals = arrival.Arrivals.Select(a => new ArrivalVM
        {
            ArrivalDisplay = a.MinutesTillArrival.ToString(),
            DepartureDateTime = a.Arrival,
            DepartureTime = a.Arrival.ToString("HH:mm"),
            Realtime = a.Realtime,
            Headsign = a.Direction,
            TripId = a.TripId,
            MinutesDisplay = true
        }).ToList();

        await Task.Yield();
        await LoadScheduledArrivalsAsync(arrival.RouteId);

        await LoadMoreArrivals();
    }

    [RelayCommand]
    public async Task LoadMoreArrivals()
    {
        if (currentPage * PAGE_SIZE >= allArrivals.Count || IsLoading)
            return;

        IsLoading = true;

        var nextItems = allArrivals.Skip(currentPage * PAGE_SIZE).Take(PAGE_SIZE).ToList();
        if (nextItems.Count > 0)
        {
            foreach (var item in nextItems)
            {
                ScheduledArrivals.Add(item);
                await Task.Yield(); // let UI update
            }
            //ScheduledArrivals.AddRange(nextItems);
            ++currentPage;
        }

        IsLoading = false;
    }

    [RelayCommand]
    private void Subscribe()
    {
        ApplicationService.SubscribeForArrival(ScheduledArrivals[0].TripId, new string(StopCode.Where(s => char.IsDigit(s)).ToArray()));
    }

    [RelayCommand]
    private void ViewRoute()
    {
        // Navigate to route/shape map
    }

    [RelayCommand]
    private void ViewVehicle()
    {
        GtfsClient.QueryVehicleUpdates();
        MapService.VehicleData = (LineName, ScheduledArrivals[0].TripId, VehicleType);
        NavigationService.ChangePage("//Main");
    }
}
