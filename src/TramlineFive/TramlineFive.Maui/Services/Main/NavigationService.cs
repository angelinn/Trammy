using GalaSoft.MvvmLight.Messaging;
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

    public async void GoToDetails(LineViewModel line)
    {
        ServiceContainer.ServiceProvider.GetService<LineDetailsViewModel>().Line = line;
        ServiceContainer.ServiceProvider.GetService<LineDetailsViewModel>().Route = line.Routes[0];
        ServiceContainer.ServiceProvider.GetService<ForwardLineDetailsViewModel>().Line = line;
        ServiceContainer.ServiceProvider.GetService<ForwardLineDetailsViewModel>().Route = line.Routes[^1];

        await Shell.Current.GoToAsync("//LineDetails");
    }
}
