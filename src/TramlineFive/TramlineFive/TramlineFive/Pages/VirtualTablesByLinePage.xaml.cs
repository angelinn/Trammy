using Rg.Plugins.Popup.Services;
using SkgtService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TramlineFive.Pages.Popup;
using TramlineFive.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TramlineFive.Pages
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class VirtualTablesByLinePage : ContentPage
	{
        public VirtualTablesByLineViewModel VirtualTablesByLineViewModel { get; private set; } = new VirtualTablesByLineViewModel();
        public VirtualTablesByLinePage ()
		{
			InitializeComponent ();
            BindingContext = VirtualTablesByLineViewModel;
		}

        private async void OnSearchClicked(object sender, EventArgs e)
        {
            IEnumerable<SkgtObject> lines = await VirtualTablesByLineViewModel.GetLinesAsync();
            ChooseByLinePopup linesPickPopup = new ChooseByLinePopup(lines);
            await PopupNavigation.PushAsync(linesPickPopup);
        }
    }
}
