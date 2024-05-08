using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

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
    public partial class SettingsViewModel : BaseViewModel
    {
        public DateTime Updated { get; private set; }

        public List<string> TileServers => TileServerSettings.TileServers.Keys.ToList();
        public List<Theme> Themes => new() { new Theme("Светла", Names.LightTheme), new Theme("Тъмна", Names.DarkTheme) };

        private Func<string, string, string, string[], Task<string>> displayActionSheet;

        public SettingsViewModel()
        {
            RefreshStopsUpdatedTime();

            ShowNearestStop = ApplicationService.GetBoolSetting(Settings.ShowStopOnLaunch, false);
            MaxTextZoom = ApplicationService.GetIntSetting(Settings.MaxTextZoom, 0);
            MaxPinsZoom = ApplicationService.GetIntSetting(Settings.MaxPinsZoom, 0);
            SelectedTileServer = ApplicationService.GetStringSetting(Settings.SelectedTileServer, TileServers.First());

            string theme = ApplicationService.GetStringSetting(Settings.Theme, Names.LightTheme);
            SelectedTheme = theme == Names.LightTheme ? Themes[0] : Themes[1];
        }

        public void Initialize(Func<string, string, string, string[], Task<string>> displayActionSheet)
        {
            this.displayActionSheet = displayActionSheet;
        }

        [RelayCommand]
        private async Task ChooseTileServer()
        {
            string result = await displayActionSheet("Избор на tile сървър", String.Empty, String.Empty, TileServers.ToArray());
            if (!String.IsNullOrEmpty(result))
                SelectedTileServer = result;
        }

        [RelayCommand]
        private async Task ChooseTheme()
        {
            string result = await displayActionSheet("Избор на тема", String.Empty, String.Empty, Themes.Select(theme => theme.Name).ToArray());
            if (!String.IsNullOrEmpty(result))
                SelectedTheme = Themes.FirstOrDefault(theme => theme.Name == result);
        }

        [ObservableProperty]
        private int maxTextZoom;
        partial void OnMaxTextZoomChanged(int value)
        {
            ApplicationService.SetIntSetting(Settings.MaxTextZoom, value);
            Messenger.Send(new SettingChanged<int>(Settings.MaxTextZoom, value));
        }

        [ObservableProperty]
        private int maxPinsZoom;
        partial void OnMaxPinsZoomChanged(int value)
        {
            ApplicationService.SetIntSetting(Settings.MaxPinsZoom, value);
            Messenger.Send(new SettingChanged<int>(Settings.MaxPinsZoom, value));
        }

        [ObservableProperty]
        private string selectedTileServer;
        partial void OnSelectedTileServerChanged(string value)
        {
            ApplicationService.SetStringSetting(Settings.SelectedTileServer, value);
            Messenger.Send(new SettingChanged<string>(Settings.SelectedTileServer, value));
        }

        [RelayCommand]
        private async Task CleanHistory()
        {
            IsLoading = true;

            await HistoryDomain.CleanHistoryAsync();

            IsLoading = false;

            Messenger.Send(new HistoryClearedMessage());
            ApplicationService.DisplayToast("Историята е изчистена");
        }

        [RelayCommand]
        private async Task UpdateStops()
        {
            IsUpdatingStops = true;

            await StopsLoader.UpdateStopsAsync();
            await StopsLoader.UpdateRoutesAsync();

            //MessengerInstance.Send<RefreshStopsMessage>();

            RefreshStopsUpdatedTime();

            ApplicationService.DisplayNotification("Tramline 5", "Спирките са обновени");

            IsUpdatingStops = false;
            OnPropertyChanged(nameof(Updated));
        }

        private void RefreshStopsUpdatedTime()
        {
            if (DateTime.TryParse(ApplicationService.GetStringSetting(Settings.StopsUpdated, null), out DateTime updated))
                Updated = updated;
        }

        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private bool isUpdatingStops;

        [ObservableProperty]
        private bool showNearestStop;
        partial void OnShowNearestStopChanged(bool value)
        {
            ApplicationService.SetBoolSetting(Settings.ShowStopOnLaunch, showNearestStop);
        }

        [ObservableProperty]
        private Theme selectedTheme;
        partial void OnSelectedThemeChanged(Theme oldValue, Theme newValue)
        {
            if (oldValue != null && newValue != null && oldValue.Value != newValue.Value)
            {
                ApplicationService.SetStringSetting(Settings.Theme, newValue.Value);
                Messenger.Send(new ChangeThemeMessage(newValue.Value));
            }
        }
    }

    public record Theme(string Name, string Value);
}
