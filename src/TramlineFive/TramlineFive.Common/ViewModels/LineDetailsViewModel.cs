using SkgtService;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using TramlineFive.Common.Models;

namespace TramlineFive.Common.ViewModels;

public class LineDetailsViewModel : BaseViewModel
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
}
