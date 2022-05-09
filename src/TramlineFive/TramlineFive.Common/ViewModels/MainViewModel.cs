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
        public ICommand OpenAboutCommand { get; private set; }

        public ICommand ShowMapCommand { get { return pages[Names.Map].Show; } set { pages[Names.Map].Show = value; } }
        public ICommand HideMapCommand { get { return pages[Names.Map].Hide; } set { pages[Names.Map].Hide = value; } }
        public ICommand ShowFavouriteCommand { get { return pages[Names.Favourites].Show; } set { pages[Names.Favourites].Show = value; } }
        public ICommand HideFavouriteCommand { get { return pages[Names.Favourites].Hide; } set { pages[Names.Favourites].Hide = value; } }
        public ICommand ShowHistoryCommand { get { return pages[Names.History].Show; } set { pages[Names.History].Show = value; } }
        public ICommand HideHistoryCommand { get { return pages[Names.History].Hide; } set { pages[Names.History].Hide = value; } }
        public ICommand AnimateMapTouchCommand { get { return pages[Names.Map].AnimateButton; } set { pages[Names.Map].AnimateButton = value; } }
        public ICommand AnimateFavouritesTouchCommand { get { return pages[Names.Favourites].AnimateButton; } set { pages[Names.Favourites].AnimateButton = value; } }
        public ICommand AnimateHistoryTouchCommand { get { return pages[Names.History].AnimateButton; } set { pages[Names.History].AnimateButton = value; } }

        private readonly Dictionary<string, ViewData> pages = new()
        {
            { Names.Map, new ViewData(true) },
            { Names.Favourites, new ViewData(false)  },
            { Names.History, new ViewData(false)  }
        };

        public MainViewModel()
        {
            ChangeViewCommand = new RelayCommand<string>((p) => ChangeView(p));
            ToggleHamburgerCommand = new RelayCommand(() => MessengerInstance.Send(new SlideHamburgerMessage()));
            OpenSettingsCommand = new RelayCommand(() => NavigationService.ChangePage("Settings"));
            OpenAboutCommand = new RelayCommand(() => NavigationService.ChangePage("About"));


            MessengerInstance.Register<ChangeThemeMessage>(this, (m) => RefreshView());
        }

        private void ChangeView(string view)
        {
            foreach (string key in pages.Keys.ToList())
            {
                if (pages[key].IsVisible)
                    pages[key].Hide.Execute(null);

                if (key == view)
                {
                    pages[key].Show.Execute(null);
                    pages[key].AnimateButton.Execute(null);
                }

                pages[key].IsVisible = key == view;
            }

            RefreshView();
        }

        private void RefreshView()
        {
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
