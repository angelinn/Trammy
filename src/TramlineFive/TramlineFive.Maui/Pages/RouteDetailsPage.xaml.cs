
using System.Threading.Tasks;
using SkgtService.Models;
using SkgtService.Models.Json;
using TramlineFive.Common.ViewModels;

namespace TramlineFive.Maui.Pages;

public partial class RouteDetailsPage : ContentPage, IQueryAttributable
{
	private object arrivalO;
	private object stopCodeO;
	private object stopNameO;

    private bool appeared = false;

	public RouteDetailsPage()
	{
		InitializeComponent();
	}

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
		if (query.TryGetValue("Arrival", out object arrival) && query.TryGetValue("stopCode", out object stopCode) && query.TryGetValue("stopName", out object stopName))
		{
			arrivalO = arrival;
            stopCodeO = stopCode;
            stopNameO = stopName;

            appeared = false;
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        (BindingContext as RouteDetailViewModel).ScheduledArrivals.Clear();

        if (!appeared)
        {
            await Task.Delay(300);
            _ = (BindingContext as RouteDetailViewModel).LoadAsync(arrivalO as RouteArrivalInformation, stopCodeO as string, stopNameO as string);
            appeared = true;
        }
    }

    private void CollectionView_RemainingItemsThresholdReached(object sender, EventArgs e)
    {

    }
}
