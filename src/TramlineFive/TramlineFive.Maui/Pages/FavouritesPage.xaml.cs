using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TramlineFive.Pages
{
	public partial class FavouritesPage : ContentPage
	{
        private bool initialized;

        public FavouritesPage ()
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
    }
}