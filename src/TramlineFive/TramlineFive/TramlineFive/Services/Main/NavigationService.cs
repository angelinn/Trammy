using System;
using System.Collections.Generic;
using System.Text;
using TramlineFive.Common.Services;
using Xamarin.Forms;

namespace TramlineFive.Services.Main;

public class NavigationService : INavigationService
{
    public async void ChangePage(string pageName)
    {
        NavigationPage main = Application.Current.MainPage as NavigationPage;

        await main.PushAsync(Activator.CreateInstance(Type.GetType($"TramlineFive.Pages.{pageName}Page")) as Page);
        await (main.RootPage as MasterPage).ToggleHamburgerAsync();
    }
}
