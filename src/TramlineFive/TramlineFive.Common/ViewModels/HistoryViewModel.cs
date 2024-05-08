using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;

using Microsoft.Extensions.DependencyInjection;
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
                ServiceContainer.ServiceProvider.GetService<MainViewModel>().ChangeViewCommand.Execute("Map");
                Messenger.Send(new StopSelectedMessage(new StopSelectedMessagePayload(value.StopCode, true)));

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

            HistoryDomain.HistoryAdded += OnHistoryAdded;
        }

        private void OnHistoryAdded(object sender, EventArgs e)
        {
            History.Insert(0, sender as HistoryDomain);
            OnPropertyChanged(nameof(HasHistory));
        }

        private void OnHistoryCleared()
        {
            History.Clear();
            OnPropertyChanged(nameof(HasHistory));
        }
    }
}
