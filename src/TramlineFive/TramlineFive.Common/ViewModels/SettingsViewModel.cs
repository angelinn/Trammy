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
using TramlineFive.Common.Models;
using TramlineFive.Common.Services;
using TramlineFive.DataAccess.Domain;

namespace TramlineFive.Common.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        public ICommand CleanHistoryCommand { get; init; }
        public ICommand UpdateStopsCommand { get; init; }
        public ICommand ChooseTileServerCommand { get; }
        public ICommand ChooseThemeCommand { get; } 

        public string UpdatedMessage => ApplicationService.GetStringSetting(Settings.StopsUpdated, null) ?? "Не е обновявано";

        public List<string> TileServers => TileServerSettings.TileServers.Keys.ToList();
        public List<Theme> Themes => new() { new Theme("Светла", Names.LightTheme), new Theme("Тъмна", Names.DarkTheme) };

        private Func<string, string, string, string[], Task<string>> displayActionSheet;

        public SettingsViewModel()
        {
            CleanHistoryCommand = new RelayCommand(async () => await CleanHistoryAsync());
            UpdateStopsCommand = new RelayCommand(async () => await ReloadStopsAsync());

            ChooseTileServerCommand = new RelayCommand(async () =>
            {
                string result = await displayActionSheet("Избор на tile сървър", String.Empty, String.Empty, TileServers.ToArray());
                if (!String.IsNullOrEmpty(result))
                    SelectedTileServer = result;
            });

            ChooseThemeCommand = new RelayCommand(async () =>
            {
                string result = await displayActionSheet("Избор на тема", String.Empty, String.Empty, Themes.Select(theme => theme.Name).ToArray());
                if (!String.IsNullOrEmpty(result))
                    SelectedTheme = Themes.FirstOrDefault(theme => theme.Name == result);
            });

            ShowNearestStop = ApplicationService.GetBoolSetting(Settings.ShowStopOnLaunch, true);
            MaxTextZoom = ApplicationService.GetIntSetting(Settings.MaxTextZoom, 0);
            MaxPinsZoom = ApplicationService.GetIntSetting(Settings.MaxPinsZoom, 0);
            SelectedTileServer = ApplicationService.GetStringSetting(Settings.SelectedTileServer, TileServerSettings.TileServers.Keys.First());

            string theme = ApplicationService.GetStringSetting(Settings.Theme, Names.LightTheme);
            SelectedTheme = theme == Names.LightTheme ? Themes[0] : Themes[1];
        }

        public void Initialize(Func<string, string, string, string[], Task<string>> displayActionSheet)
        {
            this.displayActionSheet = displayActionSheet;
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
                ApplicationService.SetBoolSetting(Settings.ShowStopOnLaunch, showNearestStop);
                RaisePropertyChanged();
            }
        }

        private Theme selectedTheme;
        public Theme SelectedTheme
        {
            get
            {
                return selectedTheme;
            }
            set
            {
                if (selectedTheme != null && value != null && selectedTheme.Value != value.Value)
                {
                    ApplicationService.SetStringSetting(Settings.Theme, value.Value);
                    MessengerInstance.Send(new ChangeThemeMessage(value.Value));
                }

                selectedTheme = value;
                RaisePropertyChanged();
            }
        }
    }

    public record Theme(string Name, string Value);
}
