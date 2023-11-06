using TramlineFive.Common.ViewModels;

namespace TramlineFive.Pages;

public partial class LinesTab : ContentPage
{
	public LinesTab()
	{
		InitializeComponent();
        search.WidthRequest = DeviceDisplay.Current.MainDisplayInfo.Width / DeviceDisplay.Current.MainDisplayInfo.Density;
    }

    protected override async void OnNavigatedTo(NavigatedToEventArgs args)
	{
		(BindingContext as LinesViewModel).SearchText = string.Empty;
		await (BindingContext as LinesViewModel).LoadAsync();
	}
}
