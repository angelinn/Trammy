using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace TramlineFive.Common.ViewModels
{
    public class AboutViewModel : BaseViewModel
    {
        private readonly LicensesViewModel licensesViewModel;

        public AboutViewModel(IServiceProvider serviceProvider)
        {
            licensesViewModel = serviceProvider.GetService<LicensesViewModel>();
        }

        private string version;

        public string Version
        {
            get
            {
                if (String.IsNullOrEmpty(version))
                    version = ApplicationService.GetVersion();

                return version;
            }
            set
            {
                version = value;
            }
        }

        public ICommand OpenLicensesPage => new RelayCommand(() =>
        {
            Task _ = licensesViewModel.Initialize();
            NavigationService.ChangePage("Licenses");
        });
    }
}
