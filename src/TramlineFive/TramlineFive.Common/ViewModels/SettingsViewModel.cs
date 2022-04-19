using GalaSoft.MvvmLight.Command;
using SkgtService;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TramlineFive.Common.Messages;
using TramlineFive.Common.Services;
using TramlineFive.DataAccess.Domain;

namespace TramlineFive.Common.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        public ICommand CleanHistoryCommand { get; init; } 
        public ICommand UpdateStopsCommand { get; init; }

        public string UpdatedMessage => ApplicationService.GetStringSetting(Settings.StopsUpdated, null) ?? "Не е обновявано";

        public List<string> TileServers => TileServerSettings.TileServers.Keys.ToList();

        public SettingsViewModel()
        {
            CleanHistoryCommand = new RelayCommand(async () => await CleanHistoryAsync());
            UpdateStopsCommand = new RelayCommand(async () => await ReloadStopsAsync());

            ShowNearestStop = ApplicationService.GetBoolSetting(Settings.ShowStopOnLaunch, true);
            MaxTextZoom = ApplicationService.GetIntSetting(Settings.MaxTextZoom, 0);
            MaxPinsZoom = ApplicationService.GetIntSetting(Settings.MaxPinsZoom, 0);
            SelectedTileServer = ApplicationService.GetStringSetting(Settings.SelectedTileServer, TileServerSettings.TileServers.Keys.First());
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

                ApplicationService.SetIntSetting(Settings.MaxTextZoom, maxTextZoom);
                MessengerInstance.Send(new SettingChanged<int>(Settings.MaxTextZoom, maxTextZoom));
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

                ApplicationService.SetIntSetting(Settings.MaxPinsZoom, maxPinsZoom);
                MessengerInstance.Send(new SettingChanged<int>(Settings.MaxPinsZoom, maxPinsZoom));
                RaisePropertyChanged(); 
            }
        }

        private string selectedTileServer;
        public string SelectedTileServer
        {
            get
            {
                return selectedTileServer;
            }
            set
            {
                selectedTileServer = value;
                ApplicationService.SetStringSetting(Settings.SelectedTileServer, selectedTileServer);
                MessengerInstance.Send(new SettingChanged<string>(Settings.SelectedTileServer, selectedTileServer));
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
