using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using NetTopologySuite.Index.HPRtree;
using Octokit;
using SkgtService;
using SkgtService.Models;
using SkgtService.Models.Json;
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
    public abstract partial class LinesViewModel : BaseViewModel
    {
        private readonly PublicTransport publicTransport;

        [ObservableProperty]
        private ObservableCollection<LineViewModel> lines;

        private List<LineViewModel> allLines;

        public string SearchText { get; set; }

        [ObservableProperty]
        private LineViewModel selectedLine;

        partial void OnSelectedLineChanged(LineViewModel value)
        {
            SelectedLine = null;

            if (value != null)
                OpenDetails(value);
        }


        public LinesViewModel(PublicTransport publicTransport)
        {
            this.publicTransport = publicTransport;
        }

        public abstract Task LoadAsync();

        protected async Task LoadTypeAsync(TransportType type)
        {
            if (allLines != null)
                return;

            await publicTransport.LoadLinesAsync();

            allLines = new(publicTransport.FindByType(type)
                .Select(p => new LineViewModel { Type = type, Routes = null, Name = p.Name })
                .OrderBy(l => {
                    if (Int32.TryParse(l.Name, out int numberName))
                        return numberName;

                    return int.MaxValue;
                    }));

            Lines = new(allLines);
        }

        private void OpenDetails(LineViewModel selected)
        {
            NavigationService.GoToDetails(selected);
        }

        [RelayCommand]
        private void FilterLines()
        {
            Lines = new ObservableCollection<LineViewModel>(allLines.Where(t => t.Name.Contains(SearchText)));
        }

    }
}
