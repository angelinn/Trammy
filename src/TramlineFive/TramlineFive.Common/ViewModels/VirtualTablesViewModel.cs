using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Sentry;
using SkgtService;
using SkgtService.Exceptions;
using SkgtService.Models;
using SkgtService.Models.Json;
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
    public partial class VirtualTablesViewModel : BaseViewModel
    {
        public ICommand AnimateFavouriteCommand { get; set; }

        private readonly ArrivalsService arrivalsService;
        private readonly VersionService versionService;
        private readonly PublicTransport publicTransport;

        private string stopCode;

        public VirtualTablesViewModel(ArrivalsService arrivalsService, VersionService versionService, PublicTransport publicTransport)
        {
            this.arrivalsService = arrivalsService;
            this.versionService = versionService;
            this.publicTransport = publicTransport;


            Messenger.Register<StopSelectedMessage>(this, async (r, sc) =>
            {
                await CheckStopAsync(sc.Value.Selected);
            });

            Messenger.Register<FavouritesChangedMessage>(this, (r, m) =>
            {
                if (StopInfo != null)
                {
                    StopInfo.IsFavourite = m.Favourites.Any(f => f.StopCode == StopInfo.Code);
                    OnPropertyChanged(nameof(StopInfo));
                }
            });
        }

        [RelayCommand]
        private async Task Refresh()
        {
            await SearchByStopCodeAsync(stopCode);
            IsRefreshing = false;
        }

        [RelayCommand]
        private void Subscribe(ArrivalInformation m)
        {
            Messenger.Send(new SubscribeMessage(m.LineName, stopCode));
        }

        [RelayCommand]
        private async Task Favourite()
        {
            FavouriteDomain added = await FavouriteDomain.AddAsync(StopInfo.PublicName, StopInfo.Code);
            if (added != null)
            {
                Messenger.Send(new FavouriteAddedMessage(added));
                ApplicationService.DisplayToast($"Спирка {added.Name} е добавена към любими");
            }
            else
            {
                ApplicationService.DisplayToast($"Спирката вече съществува в любими");
            }

            //AnimateFavouriteCommand.Execute(null);
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
                SentrySdk.CaptureException(ex);
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

                StopInformation stopInformation = publicTransport.FindStop(stopCode);

                StopResponse info = await arrivalsService.GetByStopCodeAsync(stopCode, stopInformation?.Type);

                IsLoading = false;

                FavouriteDomain favourite = await FavouriteDomain.FindAsync(info.Code);
                info.IsFavourite = favourite != null;

                StopInfo = info;

                sw.Stop();

                Messenger.Send(new ShowMapMessage(true, StopInfo.Arrivals.Count, sw.ElapsedMilliseconds));
                await HistoryDomain.AddAsync(stopCode, StopInfo.PublicName);
            }
            catch (StopNotFoundException e)
            {
                SentrySdk.CaptureException(e);
                ApplicationService.DisplayToast($"Няма данни за спирка {stopCode}.");
                IsLoading = false;
            }
            catch (Exception e)
            {
                SentrySdk.CaptureException(e);
                await ApplicationService.DisplayAlertAsync("Грешка при извличане на информация за виртуалните табла", $"Възможно е информационната система за виртуални табла да не работи. {e.GetType()} - {e.Message}", "OK");
            }
        }

        [ObservableProperty]
        private ArrivalInformation selected;
        partial void OnSelectedChanged(ArrivalInformation value)
        {
            if (value != null)
                NavigationService.GoToDetails(value, stopCode);
        }

        [ObservableProperty]
        private string direction;

        [ObservableProperty]
        private StopResponse stopInfo;

        [ObservableProperty]
        private NewVersion version;

        [ObservableProperty]
        private string message;

        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private bool isRefreshing;

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
