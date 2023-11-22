using Fizzler;
using GalaSoft.MvvmLight.Command;
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
using TramlineFive.Common.Messages;
using TramlineFive.Common.Models;
using TramlineFive.Common.Services;
using TramlineFive.Common.ViewModels;
using YamlDotNet.Core.Tokens;

namespace TramlineFive.Common.ViewModels;

public class ZoomToLineStopMessage
{
    public string Code { get; set; }
}

public class CodeViewModel : BaseViewModel
{
    public string Code { get; set; }

    private bool isHighlighted;
    public bool IsHighlighted
    {
        get => isHighlighted;
        set
        {
            isHighlighted = value;
            RaisePropertyChanged();
        }
    }
}

public class RouteViewModel
{
    public RouteViewModel(Way route)
    {
        First = route.First;
        Last = route.Last;
        Codes = route.Codes.Select(code => new CodeViewModel { Code = code }).ToList();
    }

    public string First { get; set; }
    public string Last { get; set; }
    public List<CodeViewModel> Codes { get; set; }
}

public class ScrollToHighlightedStopMessage
{
    public CodeViewModel Item { get; set; }
}

public abstract class BaseLineDetailsViewModel : BaseViewModel
{
    private readonly LineMapService lineMapService = new LineMapService();
    public LineMapService LineMapService => lineMapService;

    private LineViewModel line;
    public LineViewModel Line
    {
        get => line;
        set
        {
            line = value;
            RaisePropertyChanged();
        }
    }

    public ICommand CheckStopCommand { get; private set; }

    public BaseLineDetailsViewModel()
    {
        CheckStopCommand = new RelayCommand<CodeViewModel>((code) =>
        {
            MessengerInstance.Send(new ChangePageMessage("Map"));
            MessengerInstance.Send(new StopSelectedMessage(code.Code, true));
        });
    }

    public void SetHighlightedStop(string stop)
    {
        CodeViewModel code = route.Codes.FirstOrDefault(code => code.Code == stop);
        if (code != null)
        {
            code.IsHighlighted = true;
            MessengerInstance.Send(new ScrollToHighlightedStopMessage { Item = code });
        }
    }

    public void Load(LineViewModel line, Way route)
    {
        Line = line;
        Route = new RouteViewModel(route);

        lineMapService.SetupMapAsync(line.Name, line.Type, route);
    }

    private CodeViewModel selectedStop;
    public CodeViewModel SelectedStop
    {
        get => selectedStop;
        set
        {
            selectedStop = null;
            RaisePropertyChanged();

            if (value != null)
            {
                foreach (CodeViewModel codeVm in route.Codes)
                    codeVm.IsHighlighted = false;

                value.IsHighlighted = true;
                lineMapService.ZoomTo(value.Code);
            }
        }
    }

    public string TargetStop => Route?.Last;

    private RouteViewModel route;
    public RouteViewModel Route
    {
        get => route;
        set
        {
            route = value;
            RaisePropertyChanged();
            RaisePropertyChanged(nameof(TargetStop));
        }
    }
}

public class ForwardLineDetailsViewModel : BaseLineDetailsViewModel
{

}

public class LineDetailsViewModel : BaseLineDetailsViewModel
{

}