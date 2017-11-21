using SkgtService;
using SkgtService.Models;
using SkgtService.Parsers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using TramlineFive.DataAccess.Domain;
using TramlineFive.Services;

namespace TramlineFive.ViewModels
{
    public class Transport
    {
        public string DisplayName { get; set; }
        public TransportType TransportType { get; set; }
    }

    public class VirtualTablesByLineViewModel : BaseViewModel
    {
        public List<Transport> TransportTypes { get; private set; } = new List<Transport>()
        {
            new Transport { DisplayName = "Трамвай", TransportType = TransportType.Tram },
            new Transport { DisplayName = "Тролей", TransportType = TransportType.Trolley },
            new Transport { DisplayName = "Автобус", TransportType = TransportType.Bus }
        };
        public ObservableCollection<string> Timings { get; private set; }

        public async Task CheckForUpdatesAsync()
        {
            Version = await VersionService.CheckForUpdates();
        }

        public VirtualTablesByLineViewModel()
        {
            SelectedType = TransportTypes[0];
            SkgtManager.OnTimingsReceived += OnTimingsReceived;
        }

        private async void OnTimingsReceived(object sender, IEnumerable<string> e)
        {
            if (e != null)
            {
                Timings = new ObservableCollection<string>(e);

                OnPropertyChanged("Timings");
                OnPropertyChanged("SelectedLine");
            }
            else
            {
                Timings = null;
                //Message = "Няма часове на пристигане.";
            }
            // await HistoryDomain.AddAsync(SelectedLine, stopCode);

            OnPropertyChanged("NoTimings");
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
                OnPropertyChanged();
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
                OnPropertyChanged();
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
                OnPropertyChanged();
            }
        }

        private Transport selectedType;
        public Transport SelectedType
        {
            get
            {
                return selectedType;
            }
            set
            {
                selectedType = value;
                OnPropertyChanged();
            }
        }

        public async Task<IEnumerable<SkgtObject>> GetLinesAsync()
        {
            return await SkgtManager.LineParser.GetLinesAsync(selectedType.TransportType);
        }
    }
}
