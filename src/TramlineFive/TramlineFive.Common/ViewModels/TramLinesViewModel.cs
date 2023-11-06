using GalaSoft.MvvmLight.Command;
using NetTopologySuite.Index.HPRtree;
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

namespace TramlineFive.Common.ViewModels;

public class TramLinesViewModel : LinesViewModel
{
    public string Type => "Трамвай";
    public string Icon => "tram";
    public string IconColor => "darkorange";

    public override async Task LoadAsync()
    {
        await LoadTypeAsync("tram");
    }
}
