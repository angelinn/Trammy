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
            VersionCommand = new RelayCommand(() => ApplicationService.OpenUri(version.ReleaseUrl));
            FavouriteCommand = new RelayCommand(async () => await AddFavouriteAsync());

            MessengerInstance.Register<StopSelectedMessage>(this, async (sc) => await OnStopSelected(sc.Selected));
        }

        private async Task OnStopSelected(string stopCode)
        {
            InteractionService.ChangeTab(InteractionService.VirtualTablesIndex);
            await CheckStopAsync(stopCode);
        }

        private async Task AddFavouriteAsync()
        {
            FavouriteDomain added = await FavouriteDomain.AddAsync(stopInfo.Name, stopInfo.Code);
            if (added != null)
            {
                MessengerInstance.Send(new FavouriteAddedMessage(added));
                InteractionService.DisplayToast($"Спирка {added.Name} е добавена към любими");
            }
            else
            {
                InteractionService.DisplayToast($"Спирката вече съществува в любими");
            }
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
                await InteractionService.DisplayAlertAsync("exception", ex.InnerException.Message, "ok");
            }
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
