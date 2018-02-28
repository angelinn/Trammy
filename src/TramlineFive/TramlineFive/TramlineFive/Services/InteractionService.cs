using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TramlineFive.Common.Services;
using TramlineFive.Pages;
using Xamarin.Forms;

namespace TramlineFive.Services
{
    public class InteractionService : IInteractionService
    {
        public int VirtualTablesIndex
        {
            get
            {
                MasterDetailPage main = Application.Current.MainPage as MasterDetailPage;
                TabbedPage tabbed = (main.Detail as NavigationPage).CurrentPage as TabbedPage;

                return tabbed.Children.IndexOf(tabbed.Children.First(p => p.GetType() == typeof(VirtualTablesPage)));
            }
        }

        public async Task<bool> DisplayAlertAsync(string title, string message, string ok, string cancel)
        {
            if (!String.IsNullOrEmpty(cancel))
                return await Application.Current.MainPage.DisplayAlert(title, message, ok, cancel);

            await Application.Current.MainPage.DisplayAlert(title, message, ok);
            return true;
        }

        public void ChangeTab(int index)
        {
            MasterDetailPage main = Application.Current.MainPage as MasterDetailPage;
            TabbedPage tabbed = (main.Detail as NavigationPage).CurrentPage as TabbedPage;
            tabbed.CurrentPage = tabbed.Children[index];
        }

        public void DisplayToast(string message)
        {
            DependencyService.Get<IToastService>().ShowToast(message);
        }
    }
}
