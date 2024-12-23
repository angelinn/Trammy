using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

using SkgtService;
using SkgtService.Models.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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

        public List<string> TileServers { get; private set; }
        public List<string> FetchingStrategies => new List<string>()
        {
            "None",
            "MinimalDataFetchStrategy",
            "DataFetchStrategy"
        };

        public List<string> RenderStrategies => new List<string>()
        {
            "None",
            "RenderFetchStrategy",
            "MinimalRenderFetchStrategy",
            "TilingRenderFetchStrategy"
        };

        public List<Theme> Themes => new() { new Theme("Светла", Names.LightTheme), new Theme("Тъмна", Names.DarkTheme), new Theme("Следвай системата", Names.SystemDefault) };

        private Func<string, string, string, string[], Task<string>> displayActionSheet;

        private readonly VersionService versionService;
        private readonly PublicTransport publicTransport;

        public SettingsViewModel(VersionService versionService, PublicTransport publicTransport)
        {
            RefreshStopsUpdatedTime();

            this.versionService = versionService;
            this.publicTransport = publicTransport;
            ShowNearestStop = ApplicationService.GetBoolSetting(Settings.ShowStopOnLaunch, false);
            MaxTextZoom = ApplicationService.GetIntSetting(Settings.MaxTextZoom, 0);
            MaxPinsZoom = ApplicationService.GetIntSetting(Settings.MaxPinsZoom, 0);
            SelectedFetchingStrategy = ApplicationService.GetStringSetting(Settings.FetchingStrategy, "Full");
            SelectedTileServer = ApplicationService.GetStringSetting(Settings.SelectedTileServer, Defaults.TileServer);
            SelectedRenderStrategy = ApplicationService.GetStringSetting(Settings.RenderStrategy, "Default");

            string theme = ApplicationService.GetStringSetting(Settings.Theme, Names.SystemDefault);
            SelectedTheme = theme switch
            {
                Names.LightTheme => Themes[0],
                Names.DarkTheme => Themes[1],
                Names.SystemDefault => Themes[2],
                _ => Themes[2]
            };
        }

        public async Task Initialize(Func<string, string, string, string[], Task<string>> displayActionSheet)
        {
            this.displayActionSheet = displayActionSheet;

            await TileServerSettings.LoadTileServersAsync();
            TileServers = TileServerSettings.TileServers.Keys.ToList();
        }

        [RelayCommand]
        private async Task CheckForUpdates()
        {

            NewVersion version = await versionService.CheckForUpdates();
            if (version != null)
            {
                bool result = await ApplicationService.DisplayAlertAsync("Нова версия", $"Има нова версия {version.VersionNumber} 🎉", "СВАЛЯНЕ", "ОТКАЗ");
                if (result)
                {
                    Uri url = new Uri(version.ReleaseUrl);
                    await ApplicationService.OpenBrowserAsync(url);
                }
            }
            else
            {
                await ApplicationService.DisplayAlertAsync("", "Инсталирана е последна версия на Trammy! 🎉", "ОК");
            }
        }

        [RelayCommand]
        private async Task ChooseTileServer()
        {
            string result = await displayActionSheet("Избор на tile сървър", String.Empty, String.Empty, TileServers.ToArray());
            if (!String.IsNullOrEmpty(result))
                SelectedTileServer = result;
        }


        [RelayCommand]
        private async Task ChooseFetchingStrategy()
        {
            string result = await displayActionSheet("Избор на стратегия за Fetch", String.Empty, String.Empty, FetchingStrategies.ToArray());
            if (!String.IsNullOrEmpty(result))
                SelectedFetchingStrategy = result;
        }

        [RelayCommand]
        private async Task ChooseRenderStrategy()
        {
            string result = await displayActionSheet("Избор на стратегия за Render", String.Empty, String.Empty, RenderStrategies.ToArray());
            if (!String.IsNullOrEmpty(result))
                SelectedRenderStrategy = result;
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
        partial void OnSelectedTileServerChanging(string oldValue, string newValue)
        {
            if (!string.IsNullOrEmpty(oldValue))
            {
                ApplicationService.SetStringSetting(Settings.SelectedTileServer, newValue);
                ApplicationService.SetStringSetting(Settings.SelectedTileServerUrl, TileServerSettings.TileServers[newValue]);
                Messenger.Send(new SettingChanged<string>(Settings.SelectedTileServer, TileServerSettings.TileServers[newValue]));
            }
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

            await publicTransport.LoadData(true);

            Messenger.Send(new RefreshStopsMessage());

            RefreshStopsUpdatedTime();

            await ApplicationService.DisplayNotification("Trammy", "Спирките са обновени");

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
            ApplicationService.SetBoolSetting(Settings.ShowStopOnLaunch, ShowNearestStop);
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

        [ObservableProperty]
        private string selectedFetchingStrategy;
        partial void OnSelectedFetchingStrategyChanging(string oldValue, string newValue)
        {
            if (!string.IsNullOrEmpty(oldValue))
            {
                ApplicationService.SetStringSetting(Settings.FetchingStrategy, newValue);
                Messenger.Send(new SettingChanged<string>(Settings.FetchingStrategy, newValue));
            }
        }


        [ObservableProperty]
        private string selectedRenderStrategy;
        partial void OnSelectedRenderStrategyChanging(string oldValue, string newValue)
        {
            if (!string.IsNullOrEmpty(oldValue))
            {
                ApplicationService.SetStringSetting(Settings.RenderStrategy, newValue);
                Messenger.Send(new SettingChanged<string>(Settings.RenderStrategy, newValue));
            }
        }
    }

    public record Theme(string Name, string Value);
}
