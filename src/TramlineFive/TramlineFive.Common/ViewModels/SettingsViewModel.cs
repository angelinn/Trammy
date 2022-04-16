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

        public string UpdatedMessage => ApplicationService.GetStringSetting("StopsUpdated", null) != null ?
            ApplicationService.GetStringSetting("StopsUpdated", null) : 
            "Не е обновявано";


        public SettingsViewModel()
        {
            CleanHistoryCommand = new RelayCommand(async () => await CleanHistoryAsync());
            UpdateStopsCommand = new RelayCommand(async () => await ReloadStopsAsync());

            ShowNearestStop = ApplicationService.GetBoolSetting("ShowNearestStop", true);
            MaxTextZoom = ApplicationService.GetIntSetting("MaxTextZoom", 0);
            MaxPinsZoom = ApplicationService.GetIntSetting("MaxPinsZoom", 0);
        }

        private int maxTextZoom;
        public int MaxTextZoom
        {
            get 
            { 
                return maxTextZoom; 
            }
            set
            {
                maxTextZoom = value;

                ApplicationService.SetIntSetting("MaxTextZoom", maxTextZoom);
                MessengerInstance.Send(new SettingChanged<int>("MaxTextZoom", maxTextZoom));
                RaisePropertyChanged(); 
            }
        }

        private int maxPinsZoom; 
        public int MaxPinsZoom
        {
            get 
            { 
                return maxPinsZoom;
            }
            set 
            {
                maxPinsZoom = value;

                ApplicationService.SetIntSetting("MaxPinsZoom", maxPinsZoom);
                MessengerInstance.Send(new SettingChanged<int>("MaxPinsZoom", maxPinsZoom));
                RaisePropertyChanged(); 
            }
        }

        private async Task CleanHistoryAsync()
        {
            IsLoading = true;

            await HistoryDomain.CleanHistoryAsync();

            IsLoading = false;

            MessengerInstance.Send(new HistoryClearedMessage());
            ApplicationService.DisplayToast("Историята е изчистена");
        } 

        private async Task ReloadStopsAsync()
        {
            IsUpdatingStops = true;

            await StopsLoader.UpdateStopsAsync();
            ApplicationService.DisplayNotification("Tramline 5", "Спирките са обновени");

            IsUpdatingStops = false;
            RaisePropertyChanged("UpdatedMessage");
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

        private bool isUpdatingStops;
        public bool IsUpdatingStops
        {
            get
            {
                return isUpdatingStops;
            }
            set
            {
                isUpdatingStops = value;
                RaisePropertyChanged();
            }
        }

        private bool showNearestStop;
        public bool ShowNearestStop
        {
            get
            {
                return showNearestStop;
            }
            set
            {
                showNearestStop = value;
                ApplicationService.SetBoolSetting("ShowNearestStop", showNearestStop);
                RaisePropertyChanged();
            }
        }
    }
}
