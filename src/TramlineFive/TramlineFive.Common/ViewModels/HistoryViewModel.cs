using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;

using Microsoft.Extensions.DependencyInjection;
using SkgtService.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TramlineFive.Common.Messages;
using TramlineFive.Common.Services;
using TramlineFive.DataAccess.Domain;

namespace TramlineFive.Common.ViewModels
{
    public partial class HistoryViewModel : BaseViewModel
    {
        public ObservableCollection<HistoryDomain> History { get; private set; }

        public HistoryViewModel()
        {
            Messenger.Register<HistoryClearedMessage>(this, (r, h) => OnHistoryCleared());
            Messenger.Register<StopDataLoadedMessage>(this, async (r, s) => await OnStopDataLoadedAsync(s.stopInfo));
        }

        public bool HasHistory => (History == null || History.Count == 0) && !IsLoading;

        [ObservableProperty]
        private bool isLoading = true;

        [ObservableProperty]
        private HistoryDomain selected;

        partial void OnSelectedChanged(HistoryDomain value)
        {
            if (value != null)
            {
                Messenger.Send(new ChangePageMessage("//Map"));
                Messenger.Send(new StopSelectedMessage(value.StopCode));

                Selected = null;
                OnPropertyChanged();
            }
        }

        public async Task LoadHistoryAsync()
        {
            IsLoading = true;

            History = new ObservableCollection<HistoryDomain>(await HistoryDomain.TakeAsync());

            OnPropertyChanged(nameof(History));
            OnPropertyChanged(nameof(HasHistory));

            IsLoading = false;
        }

        private async Task OnStopDataLoadedAsync(StopResponse stopInfo)
        {
            HistoryDomain newHistory = new HistoryDomain(await HistoryDomain.AddOrUpdateHistoryAsync(stopInfo.Code, stopInfo.PublicName));
            HistoryDomain existing = History.FirstOrDefault(h => h.StopCode == newHistory.StopCode);

            if (existing is not null)
                History.Remove(existing);

            History.Insert(0, newHistory);

            OnPropertyChanged(nameof(HasHistory));
        }

        private void OnHistoryCleared()
        {
            History.Clear();
            OnPropertyChanged(nameof(HasHistory));
        }
    }
}
