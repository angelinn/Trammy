using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using TramlineFive.Common.Services;

namespace TramlineFive.Common.ViewModels
{
    public class BaseViewModel : ObservableRecipient
    {
        protected IApplicationService ApplicationService { get; private set; }
        protected INavigationService NavigationService { get; private set; }

        public BaseViewModel()
        {
            ApplicationService = ServiceContainer.ServiceProvider.GetService<IApplicationService>();
            NavigationService = ServiceContainer.ServiceProvider.GetService<INavigationService>();
        }
    }
}
