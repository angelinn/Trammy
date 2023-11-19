using Fizzler;
using SkgtService;
using SkgtService.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using TramlineFive.Common.Messages;
using TramlineFive.Common.Models;
using TramlineFive.Common.Services;
using TramlineFive.Common.ViewModels;

namespace TramlineFive.Common.ViewModels;

public class CodeViewModel
{
    public string Code { get; set; }
    public bool IsHighlighted { get; set; }
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
            selectedStop = value;
            RaisePropertyChanged();

            if (value != null)
            {
                MessengerInstance.Send(new ChangePageMessage("Map"));
                MessengerInstance.Send(new StopSelectedMessage(value.Code, true));
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