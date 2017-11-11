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
        public ObservableCollection<Line> Lines { get; private set; }
        public ObservableCollection<Direction> Directions { get; private set; }
        public ChooseByLineViewModel(IEnumerable<Line> lines)
        {
            Lines = new ObservableCollection<Line>(lines);
            SelectedLine = Lines[0];
        }

        private Line selectedLine;
        public Line SelectedLine
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

        public async Task ChooseLineAsync()
        {
            if (String.IsNullOrEmpty(selectedLine.SkgtValue))
                return;

            IsLoading = true;
            Captcha = await SkgtManager.StopCodeParser.ChooseLineAsync(selectedLine);
            CaptchaImageSource = ImageSource.FromStream(() => new MemoryStream(Captcha.BinaryContent));
            IsLoading = false;
        }

        public async Task GetDirectionsAsync()
        {
            if (String.IsNullOrEmpty(selectedLine.SkgtValue))
                return;

            IsLoading = true;
            Directions = new ObservableCollection<Direction>(await SkgtManager.LineParser.GetDirectionsAsync(selectedLine));
            IsLoading = false;
        }

        public async Task<IEnumerable<string>> GetTimingsAsync()
        {
            IsLoading = true;

            IEnumerable<string> timings = await SkgtManager.StopCodeParser.GetTimings(selectedLine, captcha.StringContent);
            SkgtManager.SendTimings(this, timings);
            IsLoading = false;

            return timings;
        }
    }
}
