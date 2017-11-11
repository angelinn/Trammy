using SkgtService.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SkgtService.Parsers
{
    public interface IStopCodeParser
    {
        Task<StopInfo> GetLinesForStopAsync(string stopCode);
        Task<Captcha> ChooseLineAsync(SkgtObject target);
        Task<IEnumerable<string>> GetTimings(SkgtObject line, string captcha);
    }
}
