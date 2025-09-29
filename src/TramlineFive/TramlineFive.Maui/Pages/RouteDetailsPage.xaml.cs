
using System.Threading.Tasks;
using SkgtService.Models;
using TramlineFive.Common.ViewModels;

namespace TramlineFive.Maui.Pages;

public partial class RouteDetailsPage : ContentPage, IQueryAttributable
{
	public RouteDetailsPage()
	{
		InitializeComponent();
	}

    public async void ApplyQueryAttributes(IDictionary<string, object> query)
    {
		if (query.TryGetValue("Arrival", out object arrival) && query.TryGetValue("stopCode", out object stopCode) && query.TryGetValue("stopName", out object stopName))
		{
			await (BindingContext as RouteDetailViewModel).LoadAsync(arrival as RouteArrivalInformation, stopCode as string, stopName as string);
		}
    }
}
