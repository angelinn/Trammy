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
        public ObservableCollection<Line> Lines { get; private set; }
        public ObservableCollection<string> Timings { get; private set; }

        private readonly ISkgtParser parser;
        public MainViewModel()
        {
            parser = new DesktopSkgtParser();
        }

        public async Task LoadLinesAsync()
        {
            Lines = new ObservableCollection<Line>(await parser.GetLinesForStopAsync(stopCode));
            OnPropertyChanged("Lines");

            SelectedLine = Lines[0];
        }
        
        public async Task ChooseLineAsync()
        {
            if (String.IsNullOrEmpty(selectedLine.SkgtValue))
                return;

            Captcha = await parser.ChooseLineAsync(selectedLine);
            CaptchaImageSource = ImageSource.FromStream(() => new MemoryStream(Captcha.BinaryContent));
        }
        
        public async Task GetTimingsAsync()
        {
            await parser.GetTimings(selectedLine, captcha.StringContent);
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
    }
}
