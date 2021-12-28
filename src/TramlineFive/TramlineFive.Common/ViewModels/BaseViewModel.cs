using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using TramlineFive.Common.Services;

namespace TramlineFive.Common.ViewModels
{
    public class BaseViewModel : ViewModelBase
    {
        protected IApplicationService ApplicationService { get; private set; }
        protected INavigationService NavigationService { get; private set; }

        public BaseViewModel()
        {
            ApplicationService = SimpleIoc.Default.GetInstance<IApplicationService>();
            NavigationService = SimpleIoc.Default.GetInstance<INavigationService>();
        }
    }
}
