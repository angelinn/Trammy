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
	}

    protected override bool OnBackButtonPressed()
    {
        Shell.Current.GoToAsync("//Lines");
        return true;
    }
}
