
using SkgtService.Models.Json;
using TramlineFive.Common.ViewModels;

namespace TramlineFive.Maui.Pages;

public partial class SchedulesPage : ContentPage, IQueryAttributable
{
	public SchedulesPage()
	{
		InitializeComponent();
	}

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
		(BindingContext as ScheduleViewModel).Load(query["route"] as RouteResponse, query["stopCode"] as string);
    }
}