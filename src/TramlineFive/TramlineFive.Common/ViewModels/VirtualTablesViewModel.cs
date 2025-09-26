using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Sentry;
using SkgtService;
using SkgtService.Exceptions;
using SkgtService.Models;
using SkgtService.Models.Json;
using SkgtService.Parsers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Topten.RichTextKit;
using TramlineFive.Common.Messages;
using TramlineFive.Common.Services;
using TramlineFive.DataAccess;
using TramlineFive.DataAccess.Domain;
using TramlineFive.DataAccess.Entities.GTFS;

namespace TramlineFive.Common.ViewModels;

public partial class VirtualTablesViewModel : BaseViewModel
{
    private readonly ArrivalsService arrivalsService;
    private readonly PublicTransport publicTransport;
    private readonly RoutesLoader routesLoader;
    private readonly GTFSClient gtfsClient;

    private string stopCode;

    public VirtualTablesViewModel(ArrivalsService arrivalsService, PublicTransport publicTransport, RoutesLoader routesLoader, GTFSClient gtfsClient)
    {
        this.arrivalsService = arrivalsService;
        this.publicTransport = publicTransport;
        this.routesLoader = routesLoader;
        this.gtfsClient = gtfsClient;

        Messenger.Register<StopSelectedMessage>(this, async (r, sc) =>
        {
            await CheckStopAsync(sc.Selected);
        });

        Messenger.Register<FavouritesChangedMessage>(this, (r, m) =>
        {
            if (StopInfo != null)
            {
                StopInfo.IsFavourite = m.Favourites.Any(f => f.StopCode == StopInfo.Code);
                OnPropertyChanged(nameof(StopInfo));
            }
        });
        this.gtfsClient = gtfsClient;
    }

    [RelayCommand]
    private async Task Refresh()
    {
        await SearchByStopCodeAsync(stopCode);
        IsRefreshing = false;
    }

    [RelayCommand]
    private void Subscribe(ArrivalInformation m)
    {
        Messenger.Send(new SubscribeMessage(m.LineName, stopCode));
    }

    [RelayCommand]
    private void Favourite()
    {
        if (!StopInfo.IsFavourite)
        {
            Messenger.Send(new RequestAddFavouriteMessage(StopInfo.PublicName, StopInfo.Code));
            ApplicationService.DisplayToast($"Спирка {StopInfo.PublicName} е добавена към любими");
        }
        else
        {
            Messenger.Send(new RequestDeleteFavouriteMessage(StopInfo.Code));
        }
    }

    public async Task CheckStopAsync(string selected)
    {
        stopCode = selected;
        await SearchByStopCodeAsync(stopCode);
    }

    public async Task SearchByStopCodeAsync(string stopCode)
    {
        StopInfo = null;
        OnPropertyChanged(nameof(StopInfo));

        List<DataAccess.Entities.GTFS.Stop> stops = await gtfsClient.GetStopsByCodeAsync(stopCode);
        StopInfo = new StopResponse(stopCode, stops[0].StopName);
        OnPropertyChanged(nameof(StopInfo));

        IsLoading = true;
        await gtfsClient.QueryRealtimeData();

        try
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();            

            List<ArrivalInformation> arrivals = new List<ArrivalInformation>();
            List<StopTimeMap> tripStopList = await GTFSContext.GetAllTripsAndStopsByStopCodeAsync(stopCode);
            foreach (StopTimeMap map in tripStopList)
            {
                if (!gtfsClient.PredictedArrivals.TryGetValue((map.TripId, map.StopId), out List<DateTime> predictedArrivals))
                    continue;

                ArrivalInformation arrival = new ArrivalInformation();
                arrival.LineName = map.RouteShortName;
                arrival.VehicleType = (TransportType)map.RouteType;
                arrival.Direction = map.TripHeadsign;
                arrival.Realtime = true;

                arrival.Arrivals.AddRange(predictedArrivals.Select(pr => new Arrival
                {
                    Realtime = true,
                    Minutes = (int)(pr - DateTime.Now).TotalMinutes,
                }));

                arrivals.Add(arrival);
            }

            if (arrivals.Count > 0)
            {
                arrivals = arrivals
                    .GroupBy(a => new { a.LineName, a.VehicleType, a.Direction })
                    .Select(g =>
                    {
                        var first = g.First();

                        return new ArrivalInformation
                        {
                            LineName = first.LineName,
                            VehicleType = first.VehicleType,
                            Direction = first.Direction,
                            Arrivals = g.SelectMany(a => a.Arrivals).OrderBy(a => a.Minutes).ToList()
                        };
                    })
                    .ToList();
            }

            //var nextDepartures = await GTFSContext.GetNextDeparturesPerStopQueryAsync(stopCode, DateTime.Now, 3);

            //foreach (var (route, departure) in nextDepartures)
            //{
            //    ArrivalInformation arrival = new ArrivalInformation();
            //    arrival.LineName = route.RouteShortName;
            //    arrival.VehicleType = (TransportType)route.RouteType;
            //    arrival.Direction = departure[0].trip.TripHeadsign;

            //        foreach (var (trip, stopTime) in departure)
            //        {
            //            if (DateTime.TryParse(stopTime.DepartureTime, out DateTime scheduledDeparture))
            //            {
            //                arrival.Arrivals.Add(new Arrival
            //                {
            //                    Minutes = (int)(scheduledDeparture - DateTime.Now).TotalMinutes,
            //                });
            //            }
            //        }

            //    arrivals.Add(arrival);
            //}

            //foreach (ArrivalInformation arrival in arrivals)
            //{
            //    List<Arrival> realtimeArrivals = [.. arrival.Arrivals.Where(a => a.Realtime)];
            //    if (realtimeArrivals.Count > 0)
            //        arrival.Arrivals = realtimeArrivals;
            //}

            StopInfo.Arrivals = [.. arrivals.OrderBy(a => a.VehicleType).ThenBy(a => {
                if (int.TryParse(a.LineName, out int lineNumber))
                    return lineNumber;

                return 1337;
            })];

            OnPropertyChanged(nameof(StopInfo));

            IsLoading = false;

            FavouriteDomain favourite = await FavouriteDomain.FindAsync(StopInfo.Code);
            StopInfo.IsFavourite = favourite != null;

            OnPropertyChanged(nameof(StopInfo));

            sw.Stop();

            Messenger.Send(new StopDataLoadedMessage(StopInfo));
        }
        catch (StopNotFoundException e)
        {
            SentrySdk.CaptureException(e);
            ApplicationService.DisplayToast($"Няма данни за спирка {stopCode}.");
            IsLoading = false;
        }
        catch (Exception e)
        {
            SentrySdk.CaptureException(e);
            await ApplicationService.DisplayAlertAsync("Грешка при извличане на информация за виртуалните табла", $"Възможно е информационната система за виртуални табла да не работи. {e.GetType()} - {e.Message}", "OK");
        }
    }

    [ObservableProperty]
    private ArrivalInformation selected;
    partial void OnSelectedChanged(ArrivalInformation value)
    {
        if (value != null)
        {
            routesLoader.LoadRoutes().ContinueWith(t =>
            {
                Route1 route = routesLoader.GetRoute(value.LineName, value.VehicleType);
                if (route is null)
                {
                    ApplicationService.RunOnUIThread(() =>
                        ApplicationService.DisplayToast($"Не е намерен маршрут за {value.VehicleType} {value.LineName}")
                    );
                    
                    return;
                }

                ApplicationService.RunOnUIThread(() =>
                    Messenger.Send(new ShowRouteMessage(route.Line, route.Direction0, value.VehicleType))
                );
            });

            Selected = null;
        }
    }

    [ObservableProperty]
    private string direction;

    [ObservableProperty]
    private StopResponse stopInfo;

    [ObservableProperty]
    private NewVersion version;

    [ObservableProperty]
    private string message;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private bool isRefreshing;
}
