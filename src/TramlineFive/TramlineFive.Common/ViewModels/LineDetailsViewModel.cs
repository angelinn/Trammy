using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

using Mapsui;
using NetTopologySuite.Triangulate;
using Newtonsoft.Json.Linq;
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
using TramlineFive.Common.Messages;
using TramlineFive.Common.Models;
using TramlineFive.Common.Services;
using TramlineFive.Common.ViewModels;
using YamlDotNet.Core.Tokens;

namespace TramlineFive.Common.ViewModels;

public enum RouteType
{
    None = -1,
    Forward,
    Backward
}

public class ZoomToLineStopMessage
{
    public string Code { get; set; }
}

public partial class CodeViewModel : BaseViewModel
{
    public string Code { get; set; }

    [ObservableProperty]
    private bool isHighlighted;
}

public class RouteViewModel
{
    public RouteViewModel(List<string> codes, string first, string last)
    {
        First = first;
        Last = last;
        Codes = codes.Select(code => new CodeViewModel { Code = code }).ToList();
    }

    public string First { get; set; }
    public string Last { get; set; }
    public List<CodeViewModel> Codes { get; set; }
}

public class ScrollToHighlightedStopMessage
{
    public CodeViewModel Item { get; set; }
}

public abstract partial class BaseLineDetailsViewModel : BaseViewModel
{
    private readonly LineMapService lineMapService;
    protected readonly PublicTransport publicTransport;

    public LineMapService LineMapService => lineMapService;

    [ObservableProperty]
    private Line line;

    [ObservableProperty]
    private List<CodeViewModel> codes;

    public BaseLineDetailsViewModel(PublicTransport publicTransport)
    {
        lineMapService = new LineMapService(publicTransport);
        this.publicTransport = publicTransport;
    }

    [RelayCommand]
    public void CheckStop(CodeViewModel code)
    {
        NavigationService.GoToSchedule(currentRoute, code.Code);
        //Messenger.Send(new ChangePageMessage("//Map"));
        //Messenger.Send(new StopSelectedMessage(new StopSelectedMessagePayload(code.Code, true)));
    }

    public async void SetHighlightedStop(string stop)
    {
        CodeViewModel code = Codes.FirstOrDefault(code => code.Code == stop);
        if (code != null)
        {
            code.IsHighlighted = true;
            Messenger.Send(new ScrollToHighlightedStopMessage { Item = code });

            await Task.Delay(1000);
            lineMapService.ZoomTo(stop);
        }
    }

    public async Task Load(Line line)
    {
        if (!publicTransport.Schedules.ContainsKey(line))
            await publicTransport.LoadSchedule(line);

        Line = line;

        currentRoute = LoadOwnLinesFromDb();
        List<string> stringCodes = currentRoute.Segments.Select(s => s.Stop.Code).ToList();

        Codes = stringCodes.Select(c => new CodeViewModel() { Code = c }).ToList();
        StopInformation stop = publicTransport.FindStop(Codes.First().Code);
        if (stop != null)
            TargetStop = stop.PublicName;

        lineMapService.SetupMap(stringCodes, line.Type);
    }

    protected RouteResponse currentRoute;
    protected abstract RouteResponse LoadOwnLinesFromDb();

    [ObservableProperty]
    private CodeViewModel selectedStop;

    partial void OnSelectedStopChanged(CodeViewModel value)
    {
        if (value != null)
        {
            foreach (CodeViewModel codeVm in Codes)
            {
                if (codeVm.Code == value.Code && codeVm.IsHighlighted)
                {
                    lineMapService.ResetView();
                    codeVm.IsHighlighted = false;

                    return;
                }

                codeVm.IsHighlighted = false;
            }

            value.IsHighlighted = true;
            lineMapService.ZoomTo(value.Code);
        }
    }

    [ObservableProperty]
    private string targetStop;
}

public class ForwardLineDetailsViewModel : BaseLineDetailsViewModel
{
    public ForwardLineDetailsViewModel(PublicTransport publicTransport) : base(publicTransport)
    { }

    protected override RouteResponse LoadOwnLinesFromDb()
    {
        ScheduleResponse schedule = publicTransport.Schedules[Line];
        List<RouteResponse> maxRoutes = schedule.Routes.OrderByDescending(r => r.Segments.Count).ToList();

        return maxRoutes[0];
    }
}

public class LineDetailsViewModel : BaseLineDetailsViewModel
{
    public LineDetailsViewModel(PublicTransport publicTransport) : base(publicTransport)
    { }

    protected override RouteResponse LoadOwnLinesFromDb()
    {
        ScheduleResponse schedule = publicTransport.Schedules[Line];
        List<RouteResponse> maxRoutes = schedule.Routes.OrderByDescending(r => r.Segments.Count).ToList();

        return maxRoutes[1];
    }
}
