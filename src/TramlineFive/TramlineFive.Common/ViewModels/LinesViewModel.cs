﻿using GalaSoft.MvvmLight.Command;
using NetTopologySuite.Index.HPRtree;
using Octokit;
using SkgtService;
using SkgtService.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TramlineFive.Common.Models;

namespace TramlineFive.Common.ViewModels
{
    public abstract class LinesViewModel : BaseViewModel
    {
        private ObservableCollection<LineViewModel> lines;
        public ObservableCollection<LineViewModel> Lines
        {
            get => lines;
            set
            {
                lines = value;
                RaisePropertyChanged();
            }
        }

        private List<LineViewModel> allLines;

        public ICommand FilterLinesCommand { get; private set; }

        public string SearchText { get; set; }

        private LineViewModel selectedLine;
        public LineViewModel SelectedLine
        {
            get => selectedLine;
            set
            {
                if (value != null)
                    OpenDetails(value);

                selectedLine = null;
                RaisePropertyChanged();
            }
        }


        public LinesViewModel()
        {
            FilterLinesCommand = new RelayCommand(FilterLines);
        }

        public abstract Task LoadAsync();

        protected async Task LoadTypeAsync(string type)
        {
            if (allLines != null)
                return;

            await StopsLoader.LoadRoutesAsync();

            allLines = new(StopsLoader.Routes.First(r => r.Key == type).Value.Select(p => new LineViewModel { Type = type, Routes = p.Value, Name = p.Key }));
            Lines = new(allLines);
        }

        private void OpenDetails(LineViewModel selected)
        {
            NavigationService.GoToDetails(selected);
        }

        private void FilterLines()
        {
            Lines = new ObservableCollection<LineViewModel>(allLines.Where(t => t.Name.Contains(SearchText)));
        }

    }
}
