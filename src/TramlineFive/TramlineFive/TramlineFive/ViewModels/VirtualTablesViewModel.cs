using SkgtService;
using SkgtService.Models;
using SkgtService.Parsers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TramlineFive.Services;
using Xamarin.Forms;

namespace TramlineFive.ViewModels
{
    public class VirtualTablesViewModel : BaseViewModel
    {
        public ObservableCollection<string> Timings { get; private set; }
        public VirtualTablesViewModel()
        {
            SkgtManager.OnTimingsReceived += OnTimingsReceived;
        }

        private void OnTimingsReceived(object sender, IEnumerable<string> e)
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
                Message = "Няма часове на пристигане.";
            }

            OnPropertyChanged("NoTimings");
        }

        public string SelectedLine
        {
            get
            {
                if (SkgtManager.SelectedLine == null)
                    return null;

                return SkgtManager.SelectedLine.DisplayName.First().ToString().ToUpper()
                            + SkgtManager.SelectedLine.DisplayName.Substring(1);
            }
        }

        public async Task<IEnumerable<Line>> LoadLinesAsync()
        {
            IsLoading = true;

            CurrentStop = await SkgtManager.Parser.GetLinesForStopAsync(stopCode);

            IsLoading = false;
            return currentStop.Lines;
        }

        public async Task CheckForUpdatesAsync()
        {
            Version = await VersionService.CheckForUpdates();
        }
        
        private Stop currentStop;
        public Stop CurrentStop
        {
            get
            {
                return currentStop;
            }
            set
            {
                currentStop = value;
                OnPropertyChanged();
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
                OnPropertyChanged();
            }
        }

        public bool NoTimings
        {
            get
            {
                return Timings == null;
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
                OnPropertyChanged();
            }
        }
    }
}
