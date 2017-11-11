using SkgtService;
using SkgtService.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using TramlineFive.DataAccess.Domain;
using Xamarin.Forms;

namespace TramlineFive.ViewModels
{
    public class ChooseLineViewModel : BaseViewModel
    {
        public ObservableCollection<SkgtObject> Lines { get; private set; }
        public ChooseLineViewModel(IEnumerable<SkgtObject> lines)
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
