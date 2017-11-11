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
using TramlineFive.DataAccess.Domain;
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
                Message = "Няма часове на пристигане.";
            }
            await HistoryDomain.AddAsync(SelectedLine, stopCode);

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

        public async Task<IEnumerable<SkgtObject>> LoadLinesAsync()
        {

            try
            {
                IsLoading = true;

                CurrentStop = await SkgtManager.StopCodeParser.GetLinesForStopAsync(stopCode);

                return currentStop.Lines;
            }
            catch (TramlineFiveException ex)
            {
                Message = ex.Message;
                return null;
            }
            finally
            {
                IsLoading = false;
            }

        }

        public async Task CheckForUpdatesAsync()
        {
            Version = await VersionService.CheckForUpdates();
        }

        private StopInfo currentStop;
        public StopInfo CurrentStop
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
