using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Fizzler;

using Mapsui;
using Newtonsoft.Json.Linq;
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

public partial class CodeViewModel : BaseViewModel
{
    public string Code { get; set; }

    [ObservableProperty]
    private bool isHighlighted;
}

public class RouteViewModel
{
    public RouteViewModel(LineRoute route, string first, string last)
    {
        First = first;
        Last = last;
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

public abstract partial class BaseLineDetailsViewModel : BaseViewModel
{
    private readonly LineMapService lineMapService;
    private readonly PublicTransport publicTransport;

    public LineMapService LineMapService => lineMapService;

    [ObservableProperty]
    private LineViewModel line;

    public BaseLineDetailsViewModel(PublicTransport publicTransport)
    {
        lineMapService = new LineMapService(publicTransport);
        this.publicTransport = publicTransport;
    }

    [RelayCommand]
    public void CheckStop(CodeViewModel code)
    {
        Messenger.Send(new ChangePageMessage("Map"));
        Messenger.Send(new StopSelectedMessage(new StopSelectedMessagePayload(code.Code, true)));
    }

    public async void SetHighlightedStop(string stop)
    {
        CodeViewModel code = route.Codes.FirstOrDefault(code => code.Code == stop);
        if (code != null)
        {
            code.IsHighlighted = true;
            Messenger.Send(new ScrollToHighlightedStopMessage { Item = code });

            await Task.Delay(1000);
            lineMapService.ZoomTo(stop);
        }
    }

    public void Load(LineViewModel line, LineRoute route)
    {
        Line = line;
        Route = new RouteViewModel(route, publicTransport.FindStop(route.Codes[0]).PublicName, publicTransport.FindStop(route.Codes[^1]).PublicName);

        lineMapService.SetupMapAsync(route, line.Type);
    }

    [ObservableProperty]
    private CodeViewModel selectedStop;

    partial void OnSelectedStopChanged(CodeViewModel value)
    {
        if (value != null)
        {
            foreach (CodeViewModel codeVm in route.Codes)
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

    public string TargetStop => Route?.Last;

    [ObservableProperty]
    private RouteViewModel route;

    partial void OnRouteChanged(RouteViewModel value)
    {
        OnPropertyChanged(nameof(TargetStop));
    }
}

public class ForwardLineDetailsViewModel : BaseLineDetailsViewModel
{
    public ForwardLineDetailsViewModel(PublicTransport publicTransport) : base(publicTransport)
    { }
}

public class LineDetailsViewModel : BaseLineDetailsViewModel
{
    public LineDetailsViewModel(PublicTransport publicTransport) : base(publicTransport)
    { }
}
