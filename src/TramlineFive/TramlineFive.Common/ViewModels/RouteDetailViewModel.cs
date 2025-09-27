using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SkgtService.Models;
using TramlineFive.DataAccess;
using TramlineFive.DataAccess.Entities.GTFS;

namespace TramlineFive.Common.ViewModels;

public class ArrivalVM
{
    public string MinutesDisplay { get; set; } // e.g. "5 min"
    public bool Realtime { get; set; }
}

public partial class RouteDetailViewModel : BaseViewModel
{
    public RouteArrivalInformation Arrival { get; private set; }
    private string stopCode;

    public ObservableCollection<ArrivalVM> ScheduledArrivals { get; set; } = new();

    [ObservableProperty]
    private bool isLoading;

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
                MinutesDisplay = $"{departureTime} - След {minutes} min",
                Realtime = false
            });
        }


        IsLoading = false;
    }

    public async Task LoadAsync(RouteArrivalInformation arrival, string stopCode)
    {
        Arrival = arrival;
        this.stopCode = stopCode;
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
        // Navigate to vehicle location page
    }
}
