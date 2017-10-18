using SkgtService;
using SkgtService.Models;
using SkgtService.Parsers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace TramlineFive.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        public ObservableCollection<string> Timings { get; private set; }
        public MainViewModel()
        {
            SkgtManager.OnTimingsReceived += OnTimingsReceived;
        }

        private void OnTimingsReceived(object sender, IEnumerable<string> e)
        {
            if (e != null)
            {
                Timings = new ObservableCollection<string>(e);
                OnPropertyChanged("Timings");
            }
            else
            {
                Timings = null;
                Message = "Няма часове на пристигане.";

                OnPropertyChanged("NoTimings");
            }
        }

        public async Task<IEnumerable<Line>> LoadLinesAsync()
        {
            IsLoading = true;

            IEnumerable<Line> lines = await SkgtManager.Parser.GetLinesForStopAsync(stopCode);

            IsLoading = false;
            return lines;
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
