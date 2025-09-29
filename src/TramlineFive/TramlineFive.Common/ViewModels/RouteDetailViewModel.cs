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
using SkgtService;
using SkgtService.Models;
using TramlineFive.Common.Services.Maps;
using TramlineFive.DataAccess;
using TramlineFive.DataAccess.Entities.GTFS;

namespace TramlineFive.Common.ViewModels;

public class ArrivalVM
{
    public int MinutesDisplay { get; set; } // e.g. "5 min"
    public string DepartureTime { get; set; }
    public bool Realtime { get; set; }
    public string Headsign { get; set; }
}

public partial class RouteDetailViewModel : BaseViewModel
{
    public RouteArrivalInformation Arrival { get; private set; }

    [ObservableProperty]
    private string stopCode;

    [ObservableProperty]
    private string stopName;

    public ObservableCollection<ArrivalVM> ScheduledArrivals { get; set; } = new();

    [ObservableProperty]
    private bool isLoading;

    private readonly GTFSClient GtfsClient;
    public RouteDetailViewModel(GTFSClient gtfsClient)
    { 
        GtfsClient = gtfsClient;
    }

    private async Task LoadScheduledArrivalsAsync()
    {
        IsLoading = true;
        List<(Trip, StopTime)> departures = await GTFSContext.GetNextDeparturesForRouteAtStopAsync(Arrival.RouteId, stopCode, DateTime.Now, 10);
        foreach ((Trip trip, StopTime stopTime) in departures)
        {
            TimeSpan ts = TimeSpan.Parse(stopTime.DepartureTime);
            DateTime departureTime = DateTime.Today.Add(ts);
            if (departureTime < DateTime.Now)
                departureTime = departureTime.AddDays(1); // handle past midnight times
            int minutes = (int)(departureTime - DateTime.Now).TotalMinutes;

            ScheduledArrivals.Add(new ArrivalVM
            {
                MinutesDisplay = minutes,
                DepartureTime = departureTime.Date == DateTime.Today ? departureTime.ToString("HH:mm") : departureTime.ToString("dd MMM HH:mm"),
                Realtime = false,
                Headsign = trip.TripHeadsign
            });
        }


        IsLoading = false;
    }

    public async Task LoadAsync(RouteArrivalInformation arrival, string stopCode, string stopName)
    {
        Arrival = arrival;
        StopCode = stopCode;
        StopName = stopName;
        ScheduledArrivals.Clear();

        OnPropertyChanged(nameof(Arrival));

        await LoadScheduledArrivalsAsync();
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
        MapService.VehicleTripId = (Arrival.LineName, Arrival.Arrivals[0].TripId);
        NavigationService.ChangePage("//Main");
    }
}
