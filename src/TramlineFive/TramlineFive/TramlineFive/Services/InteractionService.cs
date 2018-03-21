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
        public async Task<bool> DisplayAlertAsync(string title, string message, string ok, string cancel)
        {
            if (!String.IsNullOrEmpty(cancel))
                return await Application.Current.MainPage.DisplayAlert(title, message, ok, cancel);

            await Application.Current.MainPage.DisplayAlert(title, message, ok);
            return true;
        }

        public void DisplayToast(string message)
        {
            DependencyService.Get<IToastService>().ShowToast(message);
        }
    }
}
