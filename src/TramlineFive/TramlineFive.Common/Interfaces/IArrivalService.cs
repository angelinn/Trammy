using SkgtService.Models;
using SkgtService.Models.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TramlineFive.Common.Interfaces;

public interface IArrivalService
{
    Task<StopInfo> GetByStopCodeAsync(string stopCode);
}
