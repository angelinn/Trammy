using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TramlineFive.Pages
{
	public partial class AboutPage : ContentPage
	{
		public AboutPage ()
		{
			InitializeComponent ();
		}

        protected override bool OnBackButtonPressed()
        {
            Shell.Current.GoToAsync("//Main");
            return true;
        }
    }
}