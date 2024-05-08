using CommunityToolkit.Mvvm.Input;

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace TramlineFive.Common.ViewModels
{
    public partial class HamburgerViewModel : BaseViewModel
    {
        [RelayCommand]
        public void About() => NavigationService.ChangePage("About");

        [RelayCommand]
        public void Settings() => NavigationService.ChangePage("Settings");
    }
}
