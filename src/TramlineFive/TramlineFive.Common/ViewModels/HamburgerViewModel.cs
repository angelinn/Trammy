using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace TramlineFive.Common.ViewModels
{
    public class HamburgerViewModel : BaseViewModel
    {
        public ICommand AboutCommand { get; private set; }
        public ICommand SettingsCommand { get; private set; }

        public HamburgerViewModel()
        {
            AboutCommand = new RelayCommand(() => NavigationService.ChangePage("About"));
            SettingsCommand = new RelayCommand(() => NavigationService.ChangePage("Settings"));
        }
    }
}
