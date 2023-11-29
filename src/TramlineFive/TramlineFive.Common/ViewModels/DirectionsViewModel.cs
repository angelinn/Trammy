using GalaSoft.MvvmLight.Command;
using NetTopologySuite.Index.HPRtree;
using SkgtService;
using SkgtService.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TramlineFive.Common.Models;
using TramlineFive.Common.Services;

namespace TramlineFive.Common.ViewModels;

public class DirectionStepViewModel : BaseViewModel
{
    public LineInformation Line { get; set; }
    public List<StopInformation> Stops { get; set; } = new();
}

public class DirectionsViewModel : BaseViewModel
{
    private readonly DirectionsService directionsService;
    private readonly PublicTransport publicTransport;

    public ObservableCollection<DirectionStepViewModel> Directions { get; set; } = new();

    private string from = "2193";
    public string From
    {
        get => from;
        set
        {
            from = value;
            RaisePropertyChanged();
        }
    }

    private string to = "2327";
    public string To
    {
        get => to;
        set
        {
            to = value;
            RaisePropertyChanged();
        }
    }

    private bool isLoading;
    public bool IsLoading
    {
        get => isLoading;
        set
        {
            isLoading = value;
            RaisePropertyChanged();
        }
    }


    private float sameStopChangeW;
    public float SameStopChangeW
    {
        get => sameStopChangeW;
        set
        {
            sameStopChangeW = value;
            RaisePropertyChanged();
        }
    }


    private float walkToStopW;
    public float WalkToStopW
    {
        get => walkToStopW;
        set
        {
            walkToStopW = value;
            RaisePropertyChanged();
        }
    }

    public ICommand SearchCommand { get; set; }

    public DirectionsViewModel(DirectionsService directionsService, PublicTransport publicTransport)
    {
        this.directionsService = directionsService;
        this.publicTransport = publicTransport;

        SearchCommand = new RelayCommand(async () => await Search());
    }

    private bool isBuilt;
    private async Task Search()
    {
        IsLoading = true;
        if (!isBuilt)
        {

            await directionsService.BuildAsync();

            isBuilt = true;
        }
        else
            await Task.Delay(100);

        StopInformation fromStop = publicTransport.FindStop(from);
        StopInformation toStop = publicTransport.FindStop(to);

        if (fromStop == null)
        {
            ApplicationService.DisplayToast($"Спирка {from} не е валидна");
            return;
        }

        if (toStop == null)
        {
            ApplicationService.DisplayToast($"Спирка {to} не е валидна");
            return;
        }

        List<DirectionsStep> path = directionsService.GetShortestPath(fromStop, toStop).ToList();

        IsLoading = false;

        Directions.Clear();
        DirectionStepViewModel viewModel = new();

        for (int i = 0; i < path.Count; ++i)
        {
            if (i == 0 && i + 1 < path.Count)
            {
                path[0].FromLine = path[1].FromLine;
                path[0].ToLine = path[1].ToLine;
            }

            if (viewModel.Line == null)
            {
                if (viewModel.Stops.Count == 0)
                    viewModel.Line = path[i].FromLine;
            }

            if (i == path.Count - 1)
            {
                viewModel.Stops.Add(path[i].ToStop);

                Directions.Add(viewModel);
                break;
            }


            if (path[i].FromLine == viewModel.Line)
            {
                viewModel.Stops.Add(path[i].FromStop);

                if (path[i].FromLine == null)
                    viewModel.Stops.Add(path[i].ToStop);

                if (path[i].ToLine != viewModel.Line)
                {
                    if (path[i].ToLine == null)
                        viewModel.Stops.Add(path[i].ToStop);

                    Directions.Add(viewModel);
                    viewModel = new();
                }
            }
            else
            {
                if (i > 0)
                    viewModel.Stops.Add(path[i - 1].ToStop);

                //viewModel.Stops = viewModel.Stops.DistinctBy(s => s.Code).ToList();
                Directions.Add(viewModel);
                viewModel = new();

                if (path[i].FromLine == null)
                {
                    viewModel.Stops.Add(path[i].ToStop);
                    Directions.Add(viewModel);
                    viewModel = new();
                }
            }
        }
    }
}
