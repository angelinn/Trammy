using System;
using System.Collections.Generic;
using System.Text;
using TramlineFive.Common.Services;
using Xamarin.Forms;

namespace TramlineFive.Services
{
    public class NavigationService : INavigationService
    {
        public async void ChangePage(string pageName)
        {
            MasterPage main = Application.Current.MainPage as MasterPage;
            // await main.PushAsync(Activator.CreateInstance(Type.GetType($"TramlineFive.Pages.{pageName}Page")) as Page);


            (Application.Current.MainPage as MasterDetailPage).IsPresented = false;
        }
    }
}
