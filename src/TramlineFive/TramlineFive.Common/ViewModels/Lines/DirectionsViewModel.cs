using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

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
using TramlineFive.Common.ViewModels.Locator;

namespace TramlineFive.Common.ViewModels;

public enum DirectionType
{
    None,
    Begin,
    Walk,
    Change
}

public class DirectionStepViewModel : BaseViewModel
{
    public DirectionType Type { get; set; }

    public LineInformation Line { get; set; }
    public List<StopInformation> Stops { get; set; } = new();
}

public partial class DirectionsViewModel : BaseViewModel
{
    private readonly DirectionsService directionsService;
    private readonly PublicTransport publicTransport;

    public ObservableCollection<DirectionStepViewModel> Directions { get; set; } = new();

    [ObservableProperty]
    private string from = "2193";

    [ObservableProperty]
    private string to = "2327";

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private float sameStopChangeW;

    [ObservableProperty]
    private float walkToStopW;

    public DirectionsViewModel(DirectionsService directionsService, PublicTransport publicTransport)
    {
        this.directionsService = directionsService;
        this.publicTransport = publicTransport;
    }

    private bool isBuilt;

    [RelayCommand]
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
                //path[0].FromLine = path[1].FromLine;
                //path[0].ToLine = path[1].ToLine;
            }

            if (viewModel.Line == null)
            {
                if (viewModel.Stops.Count == 0)
                {
                    viewModel.Line = path[i].FromLine;
                    viewModel.Stops.Add(path[i].FromStop);
                }
            }

            if (i == path.Count - 1)
            {
                if (path[i].FromStop != path[i].ToStop)
                    viewModel.Stops.Add(path[i].ToStop);

                if (path[i].FromLine == null && path[i].ToLine == null)
                    viewModel.Type = DirectionType.Walk;

                Directions.Add(viewModel);
                break;
            }


            if (path[i].FromLine == viewModel.Line)
            {
                //viewModel.Stops.Add(path[i].FromStop);

                //if (path[i].FromLine == null)
                if (path[i].FromLine == path[i].ToLine)
                    viewModel.Stops.Add(path[i].ToStop);
                else
                {
                    
                    if (i == 0)
                    {
                        viewModel.Type = DirectionType.Walk;

                        Directions.Add(viewModel);
                        viewModel = new();
                        viewModel.Type = DirectionType.Begin;

                        continue;
                    }

                    // if you have to walk from a previous stop to next one 
                    if (path[i].ToLine != null && path[i].FromStop != path[i].ToStop)
                        viewModel.Stops.Add(path[i].ToStop);

                    if (path[i].FromLine == null && path[i].ToLine == null)
                        viewModel.Type = DirectionType.Walk;

                    if (path[i].FromLine == null && path[i].ToLine != null && viewModel.Stops.Count == 1)
                    {
                        if (path[i].FromStop != path[i].ToStop)
                        {
                            viewModel.Type = DirectionType.Walk;
                            Directions.Add(viewModel);
                        }

                        viewModel = new();
                        viewModel.Type = DirectionType.Change;
                    }
                    else
                    {
                        Directions.Add(viewModel);
                        viewModel = new();
                    }
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
