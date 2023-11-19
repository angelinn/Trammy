using CommunityToolkit.Maui.Alerts;
using GalaSoft.MvvmLight.Messaging;
using SkgtService;
using SkgtService.Models;
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

    public void GoToDetails(Line line, string stop)
    {
        if (StopsLoader.Routes.ContainsKey(line.VehicleType) && StopsLoader.Routes[line.VehicleType].ContainsKey(line.Name))
        {
            LineViewModel lineViewModel = new LineViewModel
            {
                Name = line.Name,
                Routes = StopsLoader.Routes[line.VehicleType][line.Name],
                Type = line.VehicleType
            };

            GoToDetails(lineViewModel, stop);
        }
        else
            Toast.Make($"Не може да се отиде на спирка {stop} за {line.Name}").Show();
    }

    public async void GoToDetails(LineViewModel line, string stop)
    {
        ServiceContainer.ServiceProvider.GetService<LineDetailsViewModel>().Load(line, line.Routes[0]);
        ServiceContainer.ServiceProvider.GetService<ForwardLineDetailsViewModel>().Load(line, line.Routes[^1]);

        string tab = string.Empty;

        if (!String.IsNullOrEmpty(stop))
        {
            if (line.Routes[0].Codes.Contains(stop))
            {
                tab = "Forward";
                ServiceContainer.ServiceProvider.GetService<LineDetailsViewModel>().SetHighlightedStop(stop);
            }
            else if (line.Routes[^1].Codes.Contains(stop))
            {
                tab = "Backward";
                ServiceContainer.ServiceProvider.GetService<ForwardLineDetailsViewModel>().SetHighlightedStop(stop);
            }
        }

        await Shell.Current.GoToAsync($"//LineDetails/{tab}");
    }
}
