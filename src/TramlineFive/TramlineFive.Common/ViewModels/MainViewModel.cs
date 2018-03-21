using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using TramlineFive.Common.Messages;

namespace TramlineFive.Common.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        public ICommand SearchCommand { get; private set; }
        public ICommand CancelSearchCommand { get; private set; }

        public MainViewModel()
        {
            SearchCommand = new RelayCommand(() => ActivateSearch());
            CancelSearchCommand = new RelayCommand(() => IsSearchVisible = false);
        }

        private void ActivateSearch()
        {
            if (!isSearchVisible)
            {
                IsSearchVisible = true;
                InteractionService.ChangeTab(InteractionService.VirtualTablesIndex);
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

        private bool isSearchVisible;
        public bool IsSearchVisible
        {
            get
            {
                return isSearchVisible;
            }
            set
            {
                isSearchVisible = value;
                RaisePropertyChanged();
            }
        }
    }
}
