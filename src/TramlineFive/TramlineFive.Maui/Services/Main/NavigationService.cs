using CommunityToolkit.Maui.Alerts;
using SkgtService;
using SkgtService.Models;
using SkgtService.Models.Json;
using System;
using System.Collections.Generic;
using System.Text;
using TramlineFive.Common.Models;
using TramlineFive.Common.Services;
using TramlineFive.Common.ViewModels;
using TramlineFive.Maui;
using YamlDotNet.Core;

namespace TramlineFive.Services.Main;

public class NavigationService : INavigationService
{
    public async void ChangePage(string pageName)
    {
        NavigationPage main = Application.Current.MainPage as NavigationPage;

        await main.PushAsync(Activator.CreateInstance(Type.GetType($"TramlineFive.Pages.{pageName}Page")) as Page);
        //await (main.RootPage as MainPage).ToggleHamburgerAsync();
    }

    public void GoToDetails(ArrivalInformation line, string stop)
    {
        PublicTransport publicTransport = ServiceContainer.ServiceProvider.GetService<PublicTransport>();
        Line lineInformation = publicTransport.FindByTypeAndLine(line.VehicleType, line.LineName);
        if (lineInformation != null)
        {
            GoToDetails(line, stop);
        }
        else
            Toast.Make($"Не може да се отиде на спирка {stop} за {line.LineName}").Show();
    }

    public async Task GoToSchedule(RouteResponse route, string stopCode)
    {
        await Shell.Current.GoToAsync("schedule", new Dictionary<string, object>
        {
            ["route"] = route,
            ["stopCode"] = stopCode
        });
    }

    public async Task GoToDetails(Line line, string stop)
    {
        LineDetailsViewModel vm = ServiceContainer.ServiceProvider.GetService<LineDetailsViewModel>();
        ForwardLineDetailsViewModel fvm = ServiceContainer.ServiceProvider.GetService<ForwardLineDetailsViewModel>();

        await vm.Load(line);
        await fvm.Load(line);

        string tab = string.Empty;
        tab = "Forward";

        if (!String.IsNullOrEmpty(stop))
        {
            if (vm.Codes.Any(c => c.Code == stop))
            {
                tab = "Forward";
                ServiceContainer.ServiceProvider.GetService<LineDetailsViewModel>().SetHighlightedStop(stop);
            }
            else if (fvm.Codes.Any(c => c.Code == stop))
            {
                tab = "Backward";
                ServiceContainer.ServiceProvider.GetService<ForwardLineDetailsViewModel>().SetHighlightedStop(stop);
            }
        }

        await Shell.Current.GoToAsync($"//LineDetails/{tab}");
    }
}
