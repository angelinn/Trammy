using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace TramlineFive.Common.ViewModels;

public partial class AboutViewModel : BaseViewModel
{
    private readonly LicensesViewModel licensesViewModel;

    public AboutViewModel(IServiceProvider serviceProvider)
    {
        licensesViewModel = serviceProvider.GetService<LicensesViewModel>();
        version = ApplicationService.GetVersion();
    }

    [ObservableProperty]
    private string version;

    [RelayCommand]
    public void OpenLicensesPage()
    {
        Task _ = licensesViewModel.Initialize();
        NavigationService.ChangePage("Licenses");
    }

    [RelayCommand]
    public void OpenReport()
    {
        ApplicationService.OpenUri("https://github.com/angelinn/Trammy/issues/");
    }
}
