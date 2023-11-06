using TramlineFive.Common.Models;
using TramlineFive.Common.ViewModels;

namespace TramlineFive.Maui.Pages;

public partial class LineDetails : ContentPage, IQueryAttributable
{
    public Common.Models.LineViewModel Line { get; set; } 

    public LineDetails()
	{
		InitializeComponent();
	}

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        Line = query["Line"] as Common.Models.LineViewModel;

        BindingContext = Line;
    }
}