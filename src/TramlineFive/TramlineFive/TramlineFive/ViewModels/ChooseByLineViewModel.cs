using SkgtService;
using SkgtService.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace TramlineFive.ViewModels
{
    public class ChooseByLineViewModel : BaseViewModel
    {
        public ObservableCollection<SkgtObject> Lines { get; private set; }
        public ObservableCollection<SkgtObject> Directions { get; private set; }
        public ObservableCollection<SkgtObject> Stops { get; private set; }

        public ChooseByLineViewModel(IEnumerable<SkgtObject> lines)
        {
            Lines = new ObservableCollection<SkgtObject>(lines);
            SelectedLine = Lines[0];
        }

        private SkgtObject selectedLine;
        public SkgtObject SelectedLine
        {
            get
            {
                return selectedLine;
            }
            set
            {
                selectedLine = value;
                SkgtManager.SelectedLine = value;
                OnPropertyChanged();
            }
        }


        private SkgtObject selectedDirection;
        public SkgtObject SelectedDirection
        {
            get
            {
                return selectedDirection;
            }
            set
            {
                selectedDirection = value;
                OnPropertyChanged();
            }
        }

        private SkgtObject selectedStop;
        public SkgtObject SelectedStop
        {
            get
            {
                return selectedStop;
            }
            set
            {
                selectedStop = value;
                OnPropertyChanged();
            }
        }

        private Captcha captcha;
        public Captcha Captcha
        {
            get
            {
                return captcha;
            }
            set
            {
                captcha = value;
                OnPropertyChanged();
            }
        }


        private ImageSource captchaImageSource;
        public ImageSource CaptchaImageSource
        {
            get
            {
                return captchaImageSource;
            }
            set
            {
                captchaImageSource = value;
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

        public async Task GetDirectionsAsync()
        {
            if (String.IsNullOrEmpty(selectedLine.SkgtValue))
                return;

            IsLoading = true;
            Directions = new ObservableCollection<SkgtObject>(await SkgtManager.LineParser.GetDirectionsAsync(selectedLine));
            Directions.Insert(0, new SkgtObject("Избор на посока", null));
            SelectedDirection = Directions[0];
            OnPropertyChanged("Directions");

            IsLoading = false;
        }

        public async Task GetStopsAsync()
        {
            if (String.IsNullOrEmpty(selectedDirection.SkgtValue))
                return;

            IsLoading = true;
            Stops = new ObservableCollection<SkgtObject>(await SkgtManager.LineParser.GetStopsAsync(selectedDirection));
            OnPropertyChanged("Stops");

            IsLoading = false;
        }

        public async Task ChooseStopAsync()
        {
            if (String.IsNullOrEmpty(selectedStop.SkgtValue))
                return;

            IsLoading = true;
            Captcha = await SkgtManager.LineParser.ChooseStopAsync(selectedStop);
            CaptchaImageSource = ImageSource.FromStream(() => new MemoryStream(Captcha.BinaryContent));
            IsLoading = false;
        }

        public async Task<IEnumerable<string>> GetTimingsAsync()
        {
            IsLoading = true;

            IEnumerable<string> timings = await SkgtManager.LineParser.GetTimings(selectedStop, selectedDirection, captcha.StringContent);
            SkgtManager.SendTimings(this, timings);
            IsLoading = false;

            return timings;
        }
    }
}
