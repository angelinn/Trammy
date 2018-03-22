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
        public ICommand SearchCommand { get; private set; }
        public ICommand CancelSearchCommand { get; private set; }
        public ICommand ChangeViewCommand { get; private set; }

        private Dictionary<string, ViewData> pages = new Dictionary<string, ViewData>
        {
            { "Map", new ViewData(true, "Трамваи") },
            { "Search", new ViewData(false, "")  },
            { "Settings", new ViewData(false, "Настройки")  },
            { "Favourites", new ViewData(false, "Любими")  },
            { "History", new ViewData(false, "История")  }
        };

        public MainViewModel()
        {
            SearchCommand = new RelayCommand(() => ActivateSearch());
            CancelSearchCommand = new RelayCommand(() => ChangeView("Map"));
            ChangeViewCommand = new RelayCommand<string>((p) => ChangeView(p));
        }

        private async void ChangeView(string view)
        {
            Title = pages[view].Title;
            await Task.Delay(1);
            foreach (string key in pages.Keys.ToList())
                pages[key].IsVisible = key == view;

            RaisePropertyChanged("IsSearchVisible");
            RaisePropertyChanged("IsMapVisible");
            RaisePropertyChanged("IsSettingsVisible");
            RaisePropertyChanged("IsFavouritesVisible");
            RaisePropertyChanged("IsHistoryVisible");

        }

        private void ActivateSearch()
        {
            if (!IsSearchVisible)
            {
                IsSearchVisible = true;
                ChangeView("Search");
                MessengerInstance.Send(new FocusSearchMessage());
            }
            else
            {
                if (!String.IsNullOrEmpty(query))
                {
                    MessengerInstance.Send(new StopSelectedMessage(query));
                }
            }
        }

        private string title = "Трамваи";
        public string Title
        {
            get
            {
                return title;
            }
            set
            {
                title = value;
                RaisePropertyChanged();
            }
        }

        private string query;
        public string Query
        {
            get
            {
                return query;
            }
            set
            {
                query = value;
                RaisePropertyChanged();
            }
        }

        public bool IsSearchVisible
        {
            get
            {
                return pages["Search"].IsVisible;
            }
            set
            {
                pages["Search"].IsVisible = value;
                RaisePropertyChanged();
            }
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

        public bool IsSettingsVisible
        {
            get
            {
                return pages["Settings"].IsVisible;
            }
            set
            {
                pages["Settings"].IsVisible = value;
                RaisePropertyChanged();
            }
        }
    }
}
