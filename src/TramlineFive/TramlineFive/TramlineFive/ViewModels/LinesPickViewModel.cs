using SkgtService.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;

namespace TramlineFive.ViewModels
{
    public class LinesPickViewModel : BaseViewModel
    {
        public ObservableCollection<Line> Lines { get; private set; }
        public LinesPickViewModel(IEnumerable<Line> lines)
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
                OnPropertyChanged();
            }
        }

        public async Task ChooseLineAsync()
        {
            //if (String.IsNullOrEmpty(selectedLine.SkgtValue))
            //    return;

            //IsLoading = true;
            //Captcha = await parser.ChooseLineAsync(selectedLine);
            //CaptchaImageSource = ImageSource.FromStream(() => new MemoryStream(Captcha.BinaryContent));
            //OnPropertyChanged("HasCaptcha");
            //IsLoading = false;
        }
    }
}
