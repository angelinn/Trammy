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
using SkgtService;
using SkgtService.Models;
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


            var arrival = ScheduledArrivals.FirstOrDefault(a => a.TripId == trip.TripId);
            if (arrival != null)
            {
                arrival.Delay = (int)(arrival.DepartureDateTime - departureTime).TotalMinutes;
            }
            else
            {
                ScheduledArrivals.Add(new ArrivalVM
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
        IsLoading = true;

        LineName = arrival.LineName;
        StopCode = stopCode;
        StopName = stopName;
        VehicleType = arrival.VehicleType;

        await Task.Yield();

        ScheduledArrivals.ReplaceRange(arrival.Arrivals.Select(a => new ArrivalVM
        {
            ArrivalDisplay = a.MinutesTillArrival.ToString(),
            DepartureDateTime = a.Arrival,
            DepartureTime = a.Arrival.ToString("HH:mm"),
            Realtime = a.Realtime,
            Headsign = a.Direction,
            TripId = a.TripId,
            MinutesDisplay = true
        }));

        await Task.Yield();
        await LoadScheduledArrivalsAsync(arrival.RouteId);
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
        MapService.VehicleTripId = (StopName, ScheduledArrivals[0].TripId);
        NavigationService.ChangePage("//Main");
    }
}
