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
                MasterPage main = Application.Current.MainPage as MasterPage;

                return main.Children.IndexOf(main.Children.First(p => p.GetType() == typeof(VirtualTablesPage)));
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
            MasterPage main = Application.Current.MainPage as MasterPage;
            main.CurrentPage = main.Children[index];
        }

        public void DisplayToast(string message)
        {
            DependencyService.Get<IToastService>().ShowToast(message);
        }
    }
}
