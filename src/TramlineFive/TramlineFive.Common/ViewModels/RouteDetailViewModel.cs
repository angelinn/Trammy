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

    public static bool TryNormalize(string input, out TimeSpan result)
    {
        result = TimeSpan.Zero;

        // split by ':'
        var parts = input.Split(':');
        if (parts.Length != 3) return false;

        if (!int.TryParse(parts[0], out int hours)) return false;
        if (!int.TryParse(parts[1], out int minutes)) return false;
        if (!int.TryParse(parts[2], out int seconds)) return false;

        // normalize: convert excess hours into days
        result = new TimeSpan(hours / 24, hours % 24, minutes, seconds);
        return true;
    }

    private async Task LoadScheduledArrivalsAsync(string routeId)
    {
        List<(Trip, StopTime)> departures = await GTFSContext.GetNextDeparturesForRouteAtStopAsync(routeId, StopCode, DateTime.Now, 10);

        foreach ((Trip trip, StopTime stopTime) in departures)
        {

            TryNormalize(stopTime.DepartureTime, out TimeSpan ts);

            DateTime departureTime = DateTime.Today.Add(ts);
            if (departureTime < DateTime.Now)
                departureTime = departureTime.AddDays(1); // handle past midnight times
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

        LoadMoreArrivals();
    }

    [RelayCommand]
    public async Task LoadMoreArrivals()
    {
        if (currentPage * PAGE_SIZE >= allArrivals.Count)
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
