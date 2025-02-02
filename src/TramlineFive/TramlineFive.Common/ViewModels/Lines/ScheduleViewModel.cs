using CommunityToolkit.Mvvm.ComponentModel;
using SkgtService;
using SkgtService.Models.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TramlineFive.Common.ViewModels;

public partial class ScheduleViewModel : BaseViewModel
{
    private readonly PublicTransport publicTransport;

    public ObservableCollection<TimeResponse> ScheduleArrivals { get; set; } = new();

    public ScheduleViewModel(PublicTransport publicTransport)
    {
        this.publicTransport = publicTransport;
    }

    [ObservableProperty]
    private string stopName;

    [ObservableProperty]
    private bool isWeekend;

    private List<TimeResponse> allTimes;

    partial void OnIsWeekendChanged(bool value)
    {
        LoadTimes();
    }

    public void Load(RouteResponse route, string stopCode)
    {
        foreach (var stop in route.Segments)
        {
            if (stop.Stop.Code == stopCode)
            {
                StopName = stop.Stop.Name;
                allTimes = stop.Stop.Times;

                LoadTimes();
                break;
            }
        }
    }
    private void LoadTimes()
    {
        List<TimeResponse> times = new List<TimeResponse>();
        foreach (var time in allTimes)
        {
            if (time.Weekend == (IsWeekend ? 1 : 0))
                times.Add(time);
        }

        times.Sort((one, other) => one.Time.CompareTo(other.Time));
        ScheduleArrivals.Clear();

        foreach (var t in times)
            ScheduleArrivals.Add(t);
    }

}
