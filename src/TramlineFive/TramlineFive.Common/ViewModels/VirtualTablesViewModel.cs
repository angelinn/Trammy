using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using SkgtService;
using SkgtService.Exceptions;
using SkgtService.Models;
using SkgtService.Models.Locations;
using SkgtService.Parsers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public class VirtualTablesViewModel : BaseViewModel
    {
        public ICommand FavouriteCommand { get; private set; }
        public ICommand SearchCommand { get; private set; }
        public ICommand RefreshCommand { get; private set; }
        public ICommand SearchByCodeCommand { get; private set; } 
        
        public ICommand AnimateFavouriteCommand { get; set; }

        public List<string> FilteredStops { get; private set; }

        public VirtualTablesViewModel()
        {
            FavouriteCommand = new RelayCommand(async () => await AddFavouriteAsync());
            SearchCommand = new RelayCommand(() => MessengerInstance.Send(new StopSelectedMessage(stopCode, true)));
            SearchByCodeCommand = new RelayCommand<string>((i) => MessengerInstance.Send(new StopSelectedMessage(i, true)));
            RefreshCommand = new RelayCommand(async () => {
                await SearchByStopCodeAsync();
                IsRefreshing = false;
            }); 

            MessengerInstance.Register<StopSelectedMessage>(this, async (sc) => await OnStopSelected(sc.Selected));
            MessengerInstance.Register<SearchFocusedMessage>(this, (m) => { IsFocused = m.Focused; RaisePropertyChanged("IsSearching"); });
            MessengerInstance.Register<FavouritesChangedMessage>(this, (m) =>
            { 
                if (StopInfo != null)
                {
                    StopInfo.IsFavourite = m.Favourites.Any(f => f.StopCode == StopInfo.Code);
                    RaisePropertyChanged("StopInfo");
                }
            });
        }

        public bool IsFocused { get; set; }

        public bool IsSearching => IsFocused && !String.IsNullOrEmpty(stopCode);

        private string selectedSuggestion;
        public string SelectedSuggestion
        {
            get
            {
                return selectedSuggestion;
            }
            set
            {
                selectedSuggestion = value;

                if (!String.IsNullOrEmpty(selectedSuggestion))
                {
                    string code = selectedSuggestion.Substring(0, 4);
                    CheckStopAsync(code);
                    MessengerInstance.Send(new StopSelectedMessage(code, true));
                }

                RaisePropertyChanged();
            }
        }

        private int suggestionsHeight;
        public int SuggestionsHeight
        {
            get
            {
                return suggestionsHeight;
            }
            set
            {
                suggestionsHeight = value;
                RaisePropertyChanged();
            }
        }

        private async Task OnStopSelected(string stopCode)
        {
            await CheckStopAsync(stopCode);
        }

        private async Task AddFavouriteAsync()
        {
            FavouriteDomain added = await FavouriteDomain.AddAsync(stopInfo.Name, stopInfo.Code);
            if (added != null)
            {
                MessengerInstance.Send(new FavouriteAddedMessage(added));
                ApplicationService.DisplayToast($"Спирка {added.Name} е добавена към любими");
            }
            else
            {
                ApplicationService.DisplayToast($"Спирката вече съществува в любими");
            }

            AnimateFavouriteCommand.Execute(null);
        }

        public async Task CheckStopAsync(string selected)
        {
            StopCode = selected;
            await SearchByStopCodeAsync();
        }

        public async Task CheckForUpdatesAsync()
        {
            try
            {
                Version = await VersionService.CheckForUpdates();
            }
            catch (Exception ex)
            {
                await ApplicationService.DisplayAlertAsync("exception", ex.InnerException.Message, "ok");
            }
        }

        public async Task SearchByStopCodeAsync()
        {
            IsLoading = true;

            try
            {
                StopInfo info = await new ArrivalsService().GetByStopCodeAsync(stopCode);

                IsLoading = false;

                FavouriteDomain favourite = await FavouriteDomain.FindAsync(info.Code);
                info.IsFavourite = favourite != null;

                StopInfo = info;
                Direction = stopInfo.Lines.FirstOrDefault(l => !String.IsNullOrEmpty(l.Direction))?.Direction;

                MessengerInstance.Send(new ShowMapMessage(true, StopInfo.Lines.Count));
                await HistoryDomain.AddAsync(stopCode, stopInfo.Name);
            }
            catch (StopNotFoundException)
            {
                await ApplicationService.DisplayAlertAsync("Няма данни", $"Няма данни за спирка {stopCode}.", "OK");
            }
            catch (Exception e)
            {
                await ApplicationService.DisplayAlertAsync("Грешка", e.Message, "OK");
            }
        } 

        public void FilterStops()
        {
            if (String.IsNullOrEmpty(stopCode))
                return;

            if (Char.IsDigit(stopCode[0]))
                FilteredStops = StopsLoader.Stops.Where(s => s.Code.Contains(stopCode)).Select(s => s.Code + " " + s.PublicName).Take(5).ToList();
            else
                FilteredStops = StopsLoader.Stops.Where(s => s.PublicName.ToLower().Contains(stopCode.ToLower())).Select(s => s.Code + " " + s.PublicName).Take(5).ToList();

            RaisePropertyChanged("FilteredStops");

            SuggestionsHeight = FilteredStops.Count * 50;
        }

        private Line selected;
        public Line Selected
        {
            get
            {
                return selected;
            }
            set
            {
                selected = null;
                RaisePropertyChanged();
            }
        }

        private string direction;
        public string Direction
        {
            get
            {
                return direction;
            }
            set
            {
                direction = value;
                RaisePropertyChanged();
            }
        }

        private StopInfo stopInfo;
        public StopInfo StopInfo
        {
            get
            {
                return stopInfo;
            }
            set
            {
                stopInfo = value;
                RaisePropertyChanged();
            }
        }

        private NewVersion version;
        public NewVersion Version
        {
            get
            {
                return version;
            }
            set
            {
                version = value;
                RaisePropertyChanged();
            }
        }

        private string message;
        public string Message
        {
            get
            {
                return message;
            }
            set
            {
                message = value;
                RaisePropertyChanged();
            }
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

        private bool isRefreshing;
        public bool IsRefreshing
        { 
            get
            {
                return isRefreshing;
            }
            set
            {
                isRefreshing = value;
                RaisePropertyChanged();
            }
        }

        private string stopCode;
        public string StopCode
        {
            get
            {
                return stopCode;
            }
            set
            {
                stopCode = value;
                RaisePropertyChanged();
                RaisePropertyChanged("IsSearching");

                FilterStops();
            }
        }
    }
}
