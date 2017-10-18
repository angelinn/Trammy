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

        public async Task<IEnumerable<Line>> LoadLinesAsync()
        {
            IsLoading = true;

            IEnumerable<Line> lines = await SkgtManager.Parser.GetLinesForStopAsync(stopCode);

            IsLoading = false;
            return lines;
        }

        public async Task GetTimingsAsync()
        {
            IsLoading = true;
            Timings = new ObservableCollection<string>(await SkgtManager.Parser.GetTimings(null, null));
            OnPropertyChanged("Timings");
            IsLoading = false;
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
