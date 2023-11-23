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

public class TrolleyLinesViewModel : LinesViewModel
{
    public TrolleyLinesViewModel(PublicTransport publicTransport) : base(publicTransport)
    {   }

    public string Type => "Тролей";
    public string Icon => "directions_bus";
    public string IconColor => "royalblue";

    public override async Task LoadAsync()
    {
        await LoadTypeAsync("trolley");
    }
}
