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
using TramlineFive.Common.ViewModels;

namespace TramlineFive.Common.ViewModels;

public abstract class BaseLineDetailsViewModel : BaseViewModel
{
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

    private string selectedStop;
    public string SelectedStop
    {
        get => selectedStop;
        set
        {
            selectedStop = value;
            RaisePropertyChanged();

            if (value != null)
            {
                MessengerInstance.Send(new ChangePageMessage("Map"));
                MessengerInstance.Send(new StopSelectedMessage(value, true));
            }
        }
    }

    public string TargetStop => Route?.Last;

    private Way route;
    public Way Route
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