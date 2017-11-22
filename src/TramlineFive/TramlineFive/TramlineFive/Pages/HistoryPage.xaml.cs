using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TramlineFive.Common.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TramlineFive.Pages
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class HistoryPage : ContentPage
	{
        public HistoryViewModel HistoryViewModel { get; private set; } = new HistoryViewModel();

		public HistoryPage ()
		{
			InitializeComponent ();
            BindingContext = HistoryViewModel;
		}
    }
}