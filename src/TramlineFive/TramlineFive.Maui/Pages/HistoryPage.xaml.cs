using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TramlineFive.Common.ViewModels;



namespace TramlineFive.Pages
{
	public partial class HistoryPage : ContentPage
	{
		private bool initialized;

		public HistoryPage ()
		{
			InitializeComponent ();
		}

        protected override async void OnAppearing()
        {
			if (!initialized)
			{
				initialized = true;
				await Task.Delay(500);
			}
        }

        protected override bool OnBackButtonPressed()
        {
            Shell.Current.GoToAsync("//Map");
            return true;
        }
    }
}
