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
using System.Diagnostics;
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
        public ICommand RefreshCommand { get; private set; }

        public ICommand AnimateFavouriteCommand { get; set; }


        private readonly ArrivalsService arrivalsService;
        private readonly VersionService versionService;

        private string stopCode;

        public VirtualTablesViewModel(ArrivalsService arrivalsService, VersionService versionService)
        {
            this.arrivalsService = arrivalsService;
            this.versionService = versionService;

            FavouriteCommand = new RelayCommand(async () => await AddFavouriteAsync());
            RefreshCommand = new RelayCommand(async () =>
            {
                await SearchByStopCodeAsync(stopCode);
                IsRefreshing = false;
            });

            MessengerInstance.Register<StopSelectedMessage>(this, async (sc) =>
            {
                await CheckStopAsync(sc.Selected);
            });
            MessengerInstance.Register<FavouritesChangedMessage>(this, (m) =>
            {
                if (StopInfo != null)
                {
                    StopInfo.IsFavourite = m.Favourites.Any(f => f.StopCode == StopInfo.Code);
                    RaisePropertyChanged("StopInfo");
                }
            });
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
            stopCode = selected;
            await SearchByStopCodeAsync(stopCode);
        }

        public async Task CheckForUpdatesAsync()
        {
            try
            {
                Version = await versionService.CheckForUpdates();
            }
            catch (Exception ex)
            {
                await ApplicationService.DisplayAlertAsync("exception", ex.InnerException.Message, "ok");
            }
        }

        public async Task SearchByStopCodeAsync(string stopCode)
        {
            IsLoading = true;

            try
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();

                StopInfo info = await arrivalsService.GetByStopCodeAsync(stopCode);

                IsLoading = false;

                FavouriteDomain favourite = await FavouriteDomain.FindAsync(info.Code);
                info.IsFavourite = favourite != null;

                StopInfo = info;

                Direction = stopInfo.Lines.FirstOrDefault(l => !String.IsNullOrEmpty(l.Direction))?.Direction;

                sw.Stop();

                MessengerInstance.Send(new ShowMapMessage(true, StopInfo.Lines.Count, sw.ElapsedMilliseconds));
                await HistoryDomain.AddAsync(stopCode, stopInfo.Name);
            }
            catch (StopNotFoundException)
            {
                ApplicationService.MakeSnack($"\nНяма данни за спирка {stopCode}.\n");
                IsLoading = false;
            }
            catch (Exception e)
            {
                await ApplicationService.DisplayAlertAsync("Грешка", e.Message, "OK");
            }
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

                if (value != null)
                    NavigationService.GoToDetails(value, stopCode);
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

        private StopInfo stopInfo = new StopInfo();
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

        //private string stopCode;
        //public string StopCode
        //{
        //    get
        //    {
        //        return stopCode;
        //    }
        //    set
        //    {
        //        stopCode = value;
        //        RaisePropertyChanged();
        //        RaisePropertyChanged("IsSearching");

        //        FilterStops();
        //    }
        //}
    }
}
