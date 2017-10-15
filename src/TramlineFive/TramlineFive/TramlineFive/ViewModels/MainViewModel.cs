using SkgtService.Models;
using SkgtService.Parsers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;

namespace TramlineFive.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        public ObservableCollection<Line> Lines { get; private set; } = new ObservableCollection<Line>();
            
        private readonly ISkgtParser parser;
        public MainViewModel()
        {
            parser = new DesktopSkgtParser();
        }

        public async Task LoadLinesAsync()
        {
            Lines = new ObservableCollection<Line>(await parser.GetLinesForStopAsync(stopCode));
            OnPropertyChanged("Lines");
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

        private string selectedLine;
        public string SelectedLine
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
    }
}
