using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;


using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TramlineFive.Common.Messages;
using TramlineFive.Common.Models;

namespace TramlineFive.Common.ViewModels
{
    public partial class MainViewModel : BaseViewModel
    {
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
            Messenger.Register<ChangeThemeMessage>(this, (r, m) => RefreshView());
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.GetCultureInfo("bg");
        }

        [RelayCommand]
        private void ToggleHamburger()
        {
            Messenger.Send(new SlideHamburgerMessage());
        }

        [RelayCommand]
        private void OpenSettings()
        {
            NavigationService.ChangePage("Settings");
        }

        [RelayCommand]
        private void OpenAbout()
        {
            NavigationService.ChangePage("About");
        }

        private void RefreshView()
        {
            OnPropertyChanged(nameof(IsMapVisible));
            OnPropertyChanged(nameof(IsFavouritesVisible));
            OnPropertyChanged(nameof(IsHistoryVisible));
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
                OnPropertyChanged();
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
                OnPropertyChanged();
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
                OnPropertyChanged();
            }
        }
    }
}
