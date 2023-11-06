using System;
using System.Collections.Generic;
using System.Text;
using TramlineFive.Common.Models;
using TramlineFive.Common.Services;
using TramlineFive.Common.ViewModels;
using TramlineFive.Maui;
using TramlineFive.Maui.Pages;
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

    public async void GoToDetails(Common.Models.LineViewModel line)
    {
        Dictionary<string, object> parameters = new Dictionary<string, object>()
        {
             { "Line",  line }
        };

        await Shell.Current.GoToAsync("linedetails", parameters);

        //Page main = Application.Current.MainPage;

        //await main.PushAsync(new LineDetails(line));
    }
}
