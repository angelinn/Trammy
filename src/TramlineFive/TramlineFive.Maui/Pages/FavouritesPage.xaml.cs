using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TramlineFive.Pages
{
	public partial class FavouritesPage : ContentPage
	{
		public FavouritesPage ()
		{
			InitializeComponent ();
		}


        protected override async void OnAppearing()
        {
            await Task.Delay(500);
        }
    }
}