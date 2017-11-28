using GalaSoft.MvvmLight.Command;
using SkgtService;
using SkgtService.Exceptions;
using SkgtService.Models;
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
        public ICommand SearchByStopCodeCommand { get; private set; }
        public ICommand VersionCommand { get; private set; }
        public ICommand FavouriteCommand { get; private set; }

        public VirtualTablesViewModel()
        {
            SearchByStopCodeCommand = new RelayCommand(async () => await SearchByStopCodeAsync());
            VersionCommand = new RelayCommand(async () => await CheckForUpdatesAsync());
            FavouriteCommand = new RelayCommand(async () => await AddFavouriteAsync());

            MessengerInstance.Register<HistorySelectedMessage>(this, async (h) => await CheckHistoryAsync(h.Selected));
        }

        private async Task AddFavouriteAsync()
        {
            FavouriteDomain added = await FavouriteDomain.AddAsync(stopInfo.Name, stopInfo.Code);
            MessengerInstance.Send(new FavouriteAddedMessage(added));

            InteractionService.DisplayToast($"Спирка {added.Name} е добавена към любими");
        }

        private async Task CheckHistoryAsync(HistoryDomain e)
        {
            StopCode = e.StopCode;
            await SearchByStopCodeAsync();
        }

        public async Task CheckForUpdatesAsync()
        {
            Version = await VersionService.CheckForUpdates();
        }

        public async Task SearchByStopCodeAsync()
        {
            StopInfo = null;
            IsLoading = true;

            try
            {
                StopInfo = await new ArrivalsService().GetByStopCodeAsync(stopCode);
                Direction = stopInfo.Lines.FirstOrDefault(l => !String.IsNullOrEmpty(l.Direction))?.Direction;
                await HistoryDomain.AddAsync(stopCode, stopInfo.Name);
            }
            catch (Exception e)
            {
                await InteractionService.DisplayAlertAsync("Exception", e.Message, "OK");
            }

            IsLoading = false;
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
            }
        }
    }
}
