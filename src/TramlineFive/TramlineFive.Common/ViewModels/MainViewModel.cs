using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using TramlineFive.Common.Messages;

namespace TramlineFive.Common.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        public ICommand SearchCommand { get; private set; }
        public ICommand CancelSearchCommand { get; private set; }
        public ICommand ChangeViewCommand { get; private set; }

        private Dictionary<string, bool> pages = new Dictionary<string, bool>
        {
            { "Map", true },
            { "Search", false },
            { "Settings", false },
            { "Favourites", false },
            { "History", false }
        };

        public MainViewModel()
        {
            SearchCommand = new RelayCommand(() => ActivateSearch());
            CancelSearchCommand = new RelayCommand(() => IsSearchVisible = false);
            ChangeViewCommand = new RelayCommand<string>((p) => ChangeView(p));

            RaisePropertyChanged("IsSearchVisible");
            RaisePropertyChanged("IsMapVisible");
            RaisePropertyChanged("IsSettingsVisible");
            RaisePropertyChanged("IsFavouritesVisible");
            RaisePropertyChanged("IsHistoryVisible");
        }

        private void ChangeView(string view)
        {
            foreach (string key in pages.Keys.ToList())
                pages[key] = key == view;

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
                IsSearchVisible = false;
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
                return pages["Search"];
            }
            set
            {
                pages["Search"] = value;
                RaisePropertyChanged();
            }
        }
        
        public bool IsMapVisible
        {
            get
            {
                return pages["Map"];
            }
            set
            {
                pages["Map"] = value;
                RaisePropertyChanged();
            }
        }
        
        public bool IsFavouritesVisible
        {
            get
            {
                return pages["Favourites"];
            }
            set
            {
                pages["Favourites"] = value;
                RaisePropertyChanged();
            }
        }
        
        public bool IsHistoryVisible
        {
            get
            {
                return pages["History"];
            }
            set
            {
                pages["History"] = value;
                RaisePropertyChanged();
            }
        }
        
        public bool IsSettingsVisible
        {
            get
            {
                return pages["Settings"];
            }
            set
            {
                pages["Settings"] = value;
                RaisePropertyChanged();
            }
        }
    }
}
