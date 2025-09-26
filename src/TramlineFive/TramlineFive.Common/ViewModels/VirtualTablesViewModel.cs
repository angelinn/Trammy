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
        IsLoading = true;
        await gtfsClient.QueryRealtimeData();

        try
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            List<DataAccess.Entities.GTFS.Stop> stops = gtfsClient.GetStopsByCode(stopCode);
            StopInfo = new StopResponse(stopCode, stops[0].StopName);

            var nextDepartures = GTFSContext.GetNextDeparturesPerStopQuery(stopCode, DateTime.Now);
            foreach (var (route, departure) in nextDepartures)
            {
                ArrivalInformation arrival = new ArrivalInformation();
                arrival.LineName = route.RouteShortName;
                arrival.VehicleType = (TransportType)route.RouteType;

                foreach (var (trip, stopTime) in departure)
                {
                    arrival.Direction = trip.TripHeadsign;
                    if (gtfsClient.StopTimeCache.TryGetValue($"{trip.TripId}_{stopTime.StopId}", out DateTime predictedDeparture) && predictedDeparture > DateTime.Now)
                    {
                        arrival.Realtime = true;

                        arrival.Arrivals.Add(new Arrival
                        {
                            Minutes = (int)(predictedDeparture - DateTime.Now).TotalMinutes,
                        });
                    }
                    else
                    {
                        if (DateTime.TryParse(stopTime.DepartureTime, out DateTime scheduledDeparture))
                        {
                            arrival.Arrivals.Add(new Arrival
                            {
                                Minutes = (int)(scheduledDeparture - DateTime.Now).TotalMinutes,
                            });
                        }
                    }
                }

                StopInfo.Arrivals.Add(arrival);
            }

            //StopResponse info = await arrivalsService.GetByStopCodeAsync(stopCode, stopInformation?.Type);
            //info.Arrivals = info.Arrivals
            //    .OrderBy(a => a.TransportType)
            //    .ThenBy(a =>
            //    {
            //        if (a.LineName.Any(i => !char.IsDigit(i)))
            //            return int.MaxValue;

            //        char[] lineNumber = a.LineName.Where(i => char.IsDigit(i)).ToArray();
            //        if (int.TryParse(new string(lineNumber), out int lineInt))
            //            return lineInt;

            //        return int.MaxValue;
            //    }).ToList();

            IsLoading = false;

            FavouriteDomain favourite = await FavouriteDomain.FindAsync(StopInfo.Code);
            StopInfo.IsFavourite = favourite != null;

            sw.Stop();

            Messenger.Send(new StopDataLoadedMessage(StopInfo));
            OnPropertyChanged(nameof(StopInfo));
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
