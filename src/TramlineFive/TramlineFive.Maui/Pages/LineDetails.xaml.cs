using GalaSoft.MvvmLight.Messaging;
using TramlineFive.Common.Models;
using TramlineFive.Common.ViewModels;

namespace TramlineFive.Pages;

public partial class LineDetails : ContentPage
{
    public LineDetails()
	{
        InitializeComponent();
        Messenger.Default.Register<ScrollToHighlightedStopMessage>(this, m =>
        {
            stopsList.ScrollTo(m.Item, ScrollToPosition.Center, true);
        });

        BindingContextChanged += LineDetails_BindingContextChanged;
    }

    private void LineDetails_BindingContextChanged(object sender, EventArgs e)
    {
        if (BindingContext != null)
            (BindingContext as BaseLineDetailsViewModel).LineMapService.Map = map.Map;
    }

    protected override bool OnBackButtonPressed()
    {
        Shell.Current.GoToAsync("//Lines");
        return true;
    }
}
