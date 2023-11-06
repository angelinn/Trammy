using TramlineFive.Common.ViewModels;

namespace TramlineFive.Pages;

public partial class TrolleysTab : ContentPage
{
    public TrolleysTab()
    {
        InitializeComponent();
        search.WidthRequest = DeviceDisplay.Current.MainDisplayInfo.Width / DeviceDisplay.Current.MainDisplayInfo.Density;
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        (BindingContext as LinesViewModel).SearchText = string.Empty;
    }
}
