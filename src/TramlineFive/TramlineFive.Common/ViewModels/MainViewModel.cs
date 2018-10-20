using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TramlineFive.Common.Messages;
using TramlineFive.Common.Models;

namespace TramlineFive.Common.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        public ICommand ChangeViewCommand { get; private set; }
        public ICommand ToggleHamburgerCommand { get; private set; }
        public ICommand OpenSettingsCommand { get; private set; }

        private Dictionary<string, ViewData> pages = new Dictionary<string, ViewData>
        {
            { "Map", new ViewData(true) },
            { "Favourites", new ViewData(false)  },
            { "History", new ViewData(false)  }
        };

        public MainViewModel()
        {
            ChangeViewCommand = new RelayCommand<string>((p) => ChangeView(p));
            ToggleHamburgerCommand = new RelayCommand(() => MessengerInstance.Send(new SlideHamburgerMessage()));
            OpenSettingsCommand = new RelayCommand(() => NavigationService.ChangePage("Settings"));
        }

        private void ChangeView(string view)
        {
            foreach (string key in pages.Keys.ToList())
                pages[key].IsVisible = key == view;
            
            RaisePropertyChanged("IsMapVisible");
            RaisePropertyChanged("IsFavouritesVisible");
            RaisePropertyChanged("IsHistoryVisible");
        }

        public bool IsMapVisible
        {
            get
            {
                return pages["Map"].IsVisible;
            }
            set
            {
                pages["Map"].IsVisible = value;
                RaisePropertyChanged();
            }
        }

        public bool IsFavouritesVisible
        {
            get
            {
                return pages["Favourites"].IsVisible;
            }
            set
            {
                pages["Favourites"].IsVisible = value;
                RaisePropertyChanged();
            }
        }

        public bool IsHistoryVisible
        {
            get
            {
                return pages["History"].IsVisible;
            }
            set
            {
                pages["History"].IsVisible = value;
                RaisePropertyChanged();
            }
        }
    }
}
