//using CommunityToolkit.Mvvm.Messaging;
//using TramlineFive.Common.Models;
//using TramlineFive.Common.ViewModels;
//using Microsoft.Maui.ApplicationModel;

//namespace TramlineFive.Pages;

//public partial class LineDetails : ContentPage
//{
//    public LineDetails()
//	{
//        InitializeComponent();
//        WeakReferenceMessenger.Default.Register<ScrollToHighlightedStopMessage>(this, (r, m) =>
//        {
//            stopsList.ScrollTo(m.Item, ScrollToPosition.Center, true);
//        });

//        BindingContextChanged += LineDetails_BindingContextChanged;
//    }

//    private void LineDetails_BindingContextChanged(object sender, EventArgs e)
//    {
//        if (BindingContext != null)
//        {
//            map.Map = (BindingContext as BaseLineDetailsViewModel).LineMapService.Map;
//        }
//    }

//    protected override bool OnBackButtonPressed()
//    {
//        Shell.Current.GoToAsync("//Lines");
//        return true;
//    }
//}
