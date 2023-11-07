using TramlineFive.Common.Models;
using TramlineFive.Common.ViewModels;

namespace TramlineFive.Pages;

public partial class LineDetails : ContentPage
{
    public LineDetails()
	{
		InitializeComponent();
	}

    protected override bool OnBackButtonPressed()
    {
        Shell.Current.GoToAsync("//Lines");
        return true;
    }
}
