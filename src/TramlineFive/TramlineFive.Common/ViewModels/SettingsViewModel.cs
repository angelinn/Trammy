using GalaSoft.MvvmLight.Command;
using SkgtService;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TramlineFive.Common.Messages;
using TramlineFive.DataAccess.Domain;

namespace TramlineFive.Common.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        public ICommand CleanHistoryCommand { get; private set; } 
        public ICommand UpdateStopsCommand { get; private set; }

        public SettingsViewModel()
        {
            CleanHistoryCommand = new RelayCommand(async () => await CleanHistoryAsync());
            UpdateStopsCommand = new RelayCommand(async () => await ReloadStopsAsync());
        }

        private async Task CleanHistoryAsync()
        {
            IsLoading = true;

            await HistoryDomain.CleanHistoryAsync();

            IsLoading = false;

            MessengerInstance.Send(new HistoryClearedMessage());
            InteractionService.DisplayToast("Историята е изчистена");
        }

        private async Task ReloadStopsAsync()
        {
            await StopsLoader.UpdateStopsAsync();
            InteractionService.DisplayToast("Stops updated.");
        }

        private bool isLoading;
        public bool IsLoading
        {
            get
            {
                return isLoading;
            }
            set
            {
                isLoading = value;
                RaisePropertyChanged();
            }
        }
    }
}
